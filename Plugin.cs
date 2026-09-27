using System;
using System.Collections.Generic;
using CodeHollow.FeedReader;
using Microsoft.Extensions.Logging;
using MoBro.Plugin.SDK;
using MoBro.Plugin.SDK.Builders;
using MoBro.Plugin.SDK.Enums;
using MoBro.Plugin.SDK.Exceptions;
using MoBro.Plugin.SDK.Models;
using MoBro.Plugin.SDK.Models.Metrics;
using MoBro.Plugin.SDK.Services;

namespace MoBro.Plugin.RssReader;

public sealed class Plugin : IMoBroPlugin
{
  private const int MaxFeeds = 3;
  private const int DefaultItemCount = 5;
  private const int DefaultUpdateFrequencyMinutes = 30;

  private readonly IMoBroService _service;
  private readonly IMoBroScheduler _scheduler;
  private readonly ILogger _logger;

  private readonly List<(string Url, string Name)> _feeds;
  private readonly int _itemCount;
  private readonly TimeSpan _updateFrequency;

  public Plugin(IMoBroService service, IMoBroScheduler scheduler, IMoBroSettings settings, ILogger logger)
  {
    _service = service;
    _scheduler = scheduler;
    _logger = logger;

    // Get setting values 
    _itemCount = settings.GetValue("item_count", DefaultItemCount);
    _updateFrequency = TimeSpan.FromMinutes(settings.GetValue("update_frequency", DefaultUpdateFrequencyMinutes));

    // Parse feeds (URL + name) from settings
    _feeds = [];
    for (var i = 1; i <= MaxFeeds; i++)
    {
      // optional settings: GetValue without a default throws if the setting is not set
      var url = settings.GetValue($"feed_url_{i}", "");
      if (string.IsNullOrWhiteSpace(url)) continue;

      var name = settings.GetValue($"feed_name_{i}", "");
      if (string.IsNullOrWhiteSpace(name)) name = $"Feed {i}";

      _feeds.Add((url, name));
    }

    // Ensure at least one feed is configured
    if (_feeds.Count == 0)
    {
      throw new PluginSettingsException("At least one RSS feed URL is required");
    }
  }

  public void Init()
  {
    // Register all items + start periodic update
    _service.Register(BuildItems());
    _scheduler.Interval(() => Update(), _updateFrequency, TimeSpan.Zero);
  }

  private void Update(int? specificFeedIdx = null)
  {
    var attempted = 0;
    var failed = 0;
    Exception? lastException = null;

    for (var feedIdx = 0; feedIdx < _feeds.Count; feedIdx++)
    {
      if (specificFeedIdx.HasValue && specificFeedIdx.Value != feedIdx) continue;

      attempted++;
      try
      {
        UpdateFeed(feedIdx);
      }
      catch (Exception ex)
      {
        // log and continue, so a single failing feed does not block the others
        failed++;
        lastException = ex;
        _logger.LogError(ex, "Failed to fetch or parse RSS feed {FeedIndex}: {Url}", feedIdx + 1, _feeds[feedIdx].Url);
      }
    }

    if (attempted > 0 && failed == attempted)
    {
      throw new PluginException("Failed to fetch or parse any of the RSS feeds", lastException);
    }
  }

  private void UpdateFeed(int feedIdx)
  {
    var now = DateTime.UtcNow;
    var feed = FeedReader.ReadAsync(_feeds[feedIdx].Url).GetAwaiter().GetResult();
    var idx = 0;
    foreach (var item in feed.Items)
    {
      if (idx >= _itemCount) break;

      _service.UpdateMetricValues([
        new MetricValue(MetricId(feedIdx, idx, "title"), now, item.Title),
        new MetricValue(MetricId(feedIdx, idx, "description"), now, item.Description),
        new MetricValue(MetricId(feedIdx, idx, "link"), now, item.Link),
        new MetricValue(MetricId(feedIdx, idx, "date"), now, item.PublishingDate),
        new MetricValue(MetricId(feedIdx, idx, "author"), now, item.Author),
      ]);
      idx++;
    }

    // clear remaining slots, so they do not keep items of a previous fetch
    for (; idx < _itemCount; idx++)
    {
      _service.UpdateMetricValues([
        new MetricValue(MetricId(feedIdx, idx, "title"), now, null),
        new MetricValue(MetricId(feedIdx, idx, "description"), now, null),
        new MetricValue(MetricId(feedIdx, idx, "link"), now, null),
        new MetricValue(MetricId(feedIdx, idx, "date"), now, null),
        new MetricValue(MetricId(feedIdx, idx, "author"), now, null),
      ]);
    }

    _service.UpdateMetricValue(new MetricValue(LastUpdatedMetricId(feedIdx), now, now));
  }

  private List<IMoBroItem> BuildItems()
  {
    var list = new List<IMoBroItem>();
    var actionBuilder = MoBroItem.CreateAction()
      .WithId("refresh_feeds")
      .WithLabel("action.refresh_feeds.label", "action.refresh_feeds.desc")
      .OfNoCategory()
      .OfNoGroup()
      .WithNoMetric()
      .WithHandler(HandleRefreshAction);

    for (var i = 0; i < _feeds.Count; i++)
    {
      var feedIdx = i;
      actionBuilder.WithSetting(sb => sb
        .WithName($"feed_{feedIdx}")
        .WithLabel(_feeds[feedIdx].Name, $"RSS feed: {_feeds[feedIdx].Name} ({_feeds[feedIdx].Url})")
        .OfTypeCheckbox()
        .WithDefault(true)
        .Build()
      );
    }

    list.Add(actionBuilder.Build());

    for (var feedIdx = 0; feedIdx < _feeds.Count; feedIdx++)
    {
      var categoryId = CategoryId(feedIdx);
      var feedName = _feeds[feedIdx].Name;
      var feedUrl = _feeds[feedIdx].Url;

      // category for specific feed
      list.Add(
        MoBroItem.CreateCategory()
          .WithId(categoryId)
          .WithLabel(feedName, $"RSS feed: {feedName} ({feedUrl})")
          .Build()
      );

      // last updated metric for the feed itself
      list.Add(
        MoBroItem.CreateMetric()
          .WithId(LastUpdatedMetricId(feedIdx))
          .WithLabel("metric.last_updated.label", "metric.last_updated.desc")
          .OfType(CoreMetricType.DateTime)
          .OfCategory(categoryId)
          .OfNoGroup()
          .Build()
      );

      // metrics for specific feed
      for (var i = 0; i < _itemCount; i++)
      {
        var groupId = GroupId(feedIdx, i);
        var itemNumber = i + 1;
        list.Add(
          MoBroItem.CreateGroup()
            .WithId(groupId)
            .WithLabel($"Item {itemNumber}", $"RSS feed {feedName} item {itemNumber}")
            .Build()
        );
        list.Add(BuildMetric(feedIdx, i, "title", categoryId, groupId, CoreMetricType.Text));
        list.Add(BuildMetric(feedIdx, i, "description", categoryId, groupId, CoreMetricType.Text));
        list.Add(BuildMetric(feedIdx, i, "link", categoryId, groupId, CoreMetricType.Text));
        list.Add(BuildMetric(feedIdx, i, "date", categoryId, groupId, CoreMetricType.DateTime));
        list.Add(BuildMetric(feedIdx, i, "author", categoryId, groupId, CoreMetricType.Text));
      }
    }

    return list;
  }

  private void HandleRefreshAction(IMoBroSettings settings)
  {
    for (var i = 0; i < _feeds.Count; i++)
    {
      if (settings.GetValue($"feed_{i}", false))
      {
        Update(i);
      }
    }
  }

  private static Metric BuildMetric(
    int feedIdx,
    int idx,
    string field,
    string categoryId,
    string groupId,
    CoreMetricType type
  )
  {
    return MoBroItem.CreateMetric()
      .WithId(MetricId(feedIdx, idx, field))
      .WithLabel($"metric.{field}.label", $"metric.{field}.desc")
      .OfType(type)
      .OfCategory(categoryId)
      .OfGroup(groupId)
      .Build();
  }

  private static string CategoryId(int feedIdx) => $"category_feed_{feedIdx}";
  private static string GroupId(int feedIdx, int idx) => $"group_feed_{feedIdx}_news_{idx}";
  private static string MetricId(int feedIdx, int idx, string field) => $"feed_{feedIdx}_news_{idx}_{field}";
  private static string LastUpdatedMetricId(int feedIdx) => $"feed_{feedIdx}_last_updated";
}