# Workflow
Every tests has to be written before the implementation of the functionnality and has to be updated if obsolete.

## Branching & CI

The branching of the project will be done as follow :

    functionnality -> PR -> feature -> PR -> dev -> release PR -> main Vx.x
Meaning that a developper will work inside a functionnality branch, once the issues inside the ``functionnality`` are done they will be merged it into the ``feature``. Once all functionnalities are merged into the ``feature``, it will be merged in ``dev``. And finally once the feature goals are achieved a new version will be released in ``main``.

### CI Triggers

- Push inside a branch
  - Units Tests
- PR toward ``feature``
  - Units Tests
  - Integration tests
- PR toward ``dev``
  - Integration tests
  - Build verification
  - Performance tests
- PR toward ``main``
  - Performance tests
  - Build verification
  - (manual) E2E tests

## Responsibilities

### As a developper

As a developper your responsabilities are to ensure your code is safe, easy to read and commented for anyone who comes after you. You must write your codes properly and make sure that they are up to date and most importantly not erasing any blocking test. The responsibility of the code you write and the correction of the bug that emerges from it is solely on you.

### As a reviewer

As a reviewer you must be careful when you read the code of a fellow developper and pay proper attention to any typo and easy mistakes that you could come across. You must not half-ass your inspection and actually understand the code you reviewed as if the developper is not available you will be asked to correct it.

### As a tester

As a tester you must be thorough in your testing and make sure to test every possibility to ensure that every bug and mistakes can be catched and corrected in due time.

# Tests types

## Unit test

### Environment

Unit test will have to be done using the **Unity Test Framework** package, which extends C# test framework **NUnit** with custom functionnalities for Unity.

### Process
The tests will be executed automatically using **Unity Cloud Build** before every push to ensure that no regression happens in the code. Failure on a unit test does not block a push, but will block a pull request

The execution of the unit tests will also be available on the Unity Editor Menu, as to not have to commit and push every time there's a need to verify the regression of the code.

The file name norm is : *Test/Unit_Test/{scope}_{functionnality}.asmdef*

### What to test?

The Unit test will be used for the pure logical implementation of isolated C# module across every backend function of the project.

They are not to be executed at runtime as they have to be isolated and their purpose is to verify that each low level module by itself will work as intended through all case of execution. So the tests will be done with truncated fixed value to analyze the behavior of the module.

### Code Coverage

The Code Coverage for Unit tests should aim to be at 70% for non graphical modules.

## Integration test

Integration tests will be done in one of the two cases :

- A functionnality is complete and can be merged into a feature
- A feature is complete and can be merged into the Dev branch until a new release

### Environment

The integration tests are to be done on Unity first by using **UTF** (Unity Test Framework) to do automated test on a full sequence of events then by at least one of the developpers, in test mode and play mode, trying out the new functionnalities and features as a user.

### Process

The automated tests must cover in a system the new functionnality and feature as a whole, testing multiple case of event, with the possibility of creating sub-systems to prevent and identify errors in a particular implementation.

In the functionnality case :

- First the developper that wrote the new functionnality will write his automated test with the help of the other dev that worked on this particular feature
- If the automated test work on all cases then he can create his pull request
- If the automated test could not test everything or is judged as not complete then  colleague working on the same or similar feature (can be the same person that helped with the test writing) will do the following :
  - Test each new functionnality as part of the project
  - Test each functionnality of the feature in order of implementation
  - Test of the main functionnalities in the project outside this feature
- If each test is a success then you can validate the pull request and implement the new functionnality in the feature branch.

In the case of the integration of the feature  :

- This time one or two devs from a different feature will do the following :
  - Test the implementation of the feature and its functionnalities
  - Test each feature related to the tested one to verify  that the implementation works
  - Test finally the precedent feature to ensure that nothing went awry due to the new functionnalities.

### What to test?

Every functionnality and features inside the game are concerned by the integration tests and must be done with the utmost care.

### Code Coverage

The Code Coverage for Integration tests should aim to be at 80% for all modules.

## End-to-end test

End-to-end test will be used for the versionning of the software between the dev and main branch.

### Environment

The end to end testing will be done in the Unity play mode for the early developpement test but preferably and for all the developpement the game needs to be compiled and tested as a standalone .exe binary.

### Process

The test will be done in 2 part, first the developpers will test the version to verify that everything is stable and up-to-date by doing manual tests. Then once we're sure that everything is up to standards the version will be put on the main branch and the binary will be made accessible to our group of 20 testers, along with a notification of the new functionnalities, the fixes and changes made to the project.  They will have complete access to the binary until the next version and will report at any moment their feelings about the game, the bug and the part to enhance. Those reports will be then added to the list of issues to address in the Kanban and a part of the team will be dedicated to correct that for the next version, while the rest works on the new functionnalities.

### What to test?

As said precedently, the binary as a whole will be tested and how user friendly it is. Every component, feature and functionnality will be tested by the players to make sure everything runs smoothly and give them an optimal experience.

## Performance test

### Environment

Performance test will be done using the **Unity Profiler** which allows us to receive game metrics during test and developpement in the Unity Editor.

### Process

During each versionning, in the end-to-end tests made by developpers, a full performance test will be conducted to make sure the game is enjoyable for the players.
And devs are encourage to keep the profiler open in a tab when working in the editors to make sure that the game metrics doesn't suddenly change with the modification or addition of a functionnality.

### What to test?

At least 3 type of tests will be made and automatized, but more can get added if the need arises :

- **Load tests**: Determines how the game performs when subjected to heavy workloads
- **Stress tests**: Evaluates how the game handles unexpected situations, such as sudden increase in player activity
- **Endurance tests**: Evaluates how the game performs over long periods of time
