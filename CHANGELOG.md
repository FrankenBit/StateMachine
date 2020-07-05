# Changelog

## [1.3.0] - 2020-07-05
### Changed
- Transition is now executed after the source state has exited.
- Exceptions in transition or state callbacks should now be slightly better handled (the machine should always remain in a defined state).
- On transitions from `Any` state the callbacks will be supplied with the actual source state instead of the `Default.Any` state.

## [1.2.0] - 2020-06-29
### Changed
- Evaluating transitions with a custom condition before any transitions without a custom condition.
- Evaluating transitions from `Any` state after any transitions from the specific current state, not before.
### Removed
- Removed transitions interface that is implemented by a single class now.
- Removed exit transitions collection.

## [1.1.0] - 2020-06-26
### Added
- Added the ability to specify actions to be executed on a transition between two states.
- Added a changelog.
### Removed
- Removed exception transitions (`ButWith`) to simplify logic and because such transitions still can be modeled using other methods still available.

## [1.0.0] - 2020-06-23
- Initial version of a nestable finite state machine for Unity 3D.
