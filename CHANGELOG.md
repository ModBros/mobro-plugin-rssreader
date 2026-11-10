# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## 1.0.1 - 2026-11-10

### Changed

- Updated MoBro SDK to v1.2.0
- Updated CodeHollow.FeedReader to v2.1.1
- Item descriptions are shown as plain text without HTML markup and are shortened to 500 characters
- Feed requests time out after 30 seconds and identify themselves with a MoBro user agent
- Item group labels are now translated
- The MoBro SDK is no longer bundled with the plugin, as it is provided by MoBro

### Fixed

- A single unreachable or invalid feed no longer prevents the other feeds from updating, also when refreshing manually
- Plugin no longer fails to start when optional feed URLs or names are left empty
- Item slots are cleared when a feed returns fewer items than configured, instead of showing outdated items
- A manual refresh no longer runs at the same time as the scheduled update

## 1.0.0 - 2024-07-28

### Added

- Initial release of RSS Reader plugin
