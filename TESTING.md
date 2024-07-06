# Testing

This library is intended to be used in two ways:

1. Fully automated generation of gaming maps.
2. Creating an initial map for a human designer to build upon.

Given that in both cases the expectation is that the map will be generated
in a way usable in the game, we want to make sure that it generates a proper
map so we need to establish a comprehensive testing approach.

# How we test

The general approach to testing is:

1. 100% Unit tests coverage
2. An integration test per feature
3. Load tests for core features
4. Performance tests for core features

## Unit tests

A unit test is a test that tests one class. This is only possible when we use
DI because otherwise every unit test will also test all downstream calls.

TODO: Implement DI
TODO: Implement a test per class

Challenges:

- There is no clear separation between data and algorithms as for this library
  the algorithms heavily rely on the data. So there are class dependencies that
  are difficult to isolate via DI.

## Integration tests

Integration tests test the integration between several components of this
library, usually replicating a real user scenario within the boundaries of this
library.

These tests should include tricky scenarios testing all the ways the users will
use the library.

## Load tests

Load tests test the boundaries of the algorithms by:

- Testing all possible variations of parameters.
- Testing different scale of values (e.g., building very large or super-dense
  mazes).

## Performance tests

Performance tests should track and maintain historical performance of
Integration tests. I.e., how has the performance of integration tests changed
over time.
