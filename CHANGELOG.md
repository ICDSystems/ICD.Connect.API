# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [4.0.2] - 2019-09-03
### Changed
 - Fixed a bug when trying to execute a command and nothing is returned in console due to 0 nodes in tree

## [4.0.1] - 2019-08-15
### Changed
 - Failing gracefully when a console command uses a bad index

## [4.0.0] - 2019-02-07
### Changed
 - Minor changes to support IcdConsoleServer
 - Attribute caches are now threadsafe
 - Significantly reduced JSON footprint for serialized API info
 - Various performance improvements for API features
 
### Removed
 - Removed proxy-types as they are currently unused and impact performance
 
## [3.3.1] - 2019-04-25
### Changed
 - Clearer error messages when a console command fails to execute

## [3.3.0] - 2019-01-02
### Added
 - Support selecting console node group child by name
 - Better exceptions when API info chaining fails
 - Exposed conversion method for console string parameters

## [3.2.1] - 2018-10-04
### Changed
 - Fixed generic type representation in console

## [3.2.0] - 2018-09-14
### Added
 - Added methods for setting properties via API

### Changed
 - Small optimizations

## [3.1.0] - 2018-06-19
### Added
 - Adding support for quoted console commands/parameters

## [3.0.0] - 2018-04-23
### Added
 - Added attributes for API hooks
 - Added builder for API commands
 - Added handler for API commands 
 