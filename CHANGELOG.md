# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [Unreleased]
### Changed
 - Fixed netwonsoft references for NETFRAMEWORK vs NETSTANDARD

## [5.0.3] - 2022-07-11
### Changed
 - Fixed Preprocessor issue for S+ console

## [5.0.2] - 2022-07-01
### Changed
 - Fixed PreProcessors for NETSTANDARD vs SIMPLSHARP for 4-series builds
 - Updated Dependencies, Updated Crestron SDK to 2.18.96

## [5.0.1] - 2021-08-03
### Changed
 - Clarifying console name nullability, better handling of null/empty names

## [5.0.0] - 2021-05-14
### Added
 - Added "Is Elevated" to root console status

### Changed
 - ApiHandler logic split into relevant API nodes
 - Fixed a bug preventing empty string parameters in the console
 - Moved API root to the DirectMessageManager

## [4.2.3] - 2021-02-04
### Changed
 - Changed from Uptime to StartTime for Program/System start time

## [4.2.2] - 2020-06-18
### Changed
 - Performance improvements for console abbreviations
 - API event args implement IGenericEventArgs

## [4.2.1] - 2020-03-24
### Changed
 - Fixed some bad assumptions when generating console abbreviations

## [4.2.0] - 2020-03-20
### Added
 - ICD console supports setting/clearing root with '/' command
 - Console commands support abbreviations

### Changed
 - True/False values in the console status are now red/green

## [4.1.1] - 2019-11-18
### Changed
 - Failing gracefully when a console command tries to use an index that doesnt exist

## [4.1.0] - 2019-09-16
### Added
 - Added console command "ICD Uptime" which displays a table with the program install date, current uptime, and time since last system restart.
 - Added console command to print current API subscribers
 
### Changed
 - Fixed bugs with API subscriptions

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
 