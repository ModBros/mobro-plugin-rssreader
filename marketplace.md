# RSS Reader

The **RSS Reader Plugin** allows MoBro to integrate the latest items from up to 3 configurable RSS feeds.

---

## Features

- Display the latest items from up to 3 configurable RSS feeds
- Customizable display name per feed
- Configurable number of items shown per feed
- Automatic periodic refresh at a configurable interval
- Manual refresh of selected feeds via the "Refresh RSS feeds" action

---

## Where to Find RSS Feed URLs

Most news sites and blogs offer RSS feeds. Here are a few popular examples to get you started:

| Source                                  | RSS Feed URL                                      |
|-----------------------------------------|---------------------------------------------------|
| BBC News                                | `https://feeds.bbci.co.uk/news/rss.xml`           |
| NASA                                    | `https://www.nasa.gov/rss/dyn/breaking_news.rss`  |
| Ars Technica                            | `https://feeds.arstechnica.com/arstechnica/index` |
| Reddit (r/news)                         | `https://www.reddit.com/r/news/.rss`              |
| Hacker News                             | `https://hnrss.org/frontpage`                     |
| ORF (Austrian Broadcasting Corporation) | `https://rss.orf.at/news.xml`                     |

**Tip:** To find the RSS feed for a website, look for the RSS icon (📡) on the page or search for "RSS
feed [website name]".

---

## Settings

| Setting                        |                                 Default | Description                                                    |
|--------------------------------|----------------------------------------:|----------------------------------------------------------------|
| **RSS Feed URL 1**             | `https://feeds.bbci.co.uk/news/rss.xml` | The URL of the first RSS feed to display (required)            |
| **RSS Feed Name 1**            |                              `BBC News` | A display name for the first RSS feed                          |
| **RSS Feed URL 2**             |                                         | The URL of the second RSS feed to display (optional)           |
| **RSS Feed Name 2**            |                                `Feed 2` | A display name for the second RSS feed                         |
| **RSS Feed URL 3**             |                                         | The URL of the third RSS feed to display (optional)            |
| **RSS Feed Name 3**            |                                `Feed 3` | A display name for the third RSS feed                          |
| **Number of items per feed**   |                                     `5` | The number of feed items to display per feed (min: 1, max: 20) |
| **Update frequency (minutes)** |                                    `30` | How often to fetch the latest feed items in minutes (min: 5)   |
