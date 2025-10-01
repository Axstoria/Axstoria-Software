# Deployement strategy

## Branch workflow

The branches follows a very strict workflow and direction, thus :
Only pull request from ``dev`` can be merged into ``main``
Only pull request from a ``feature`` branch can be merged into ``dev``
And finally only an associated ``functionnality`` can be merged into  a ``feature``

Functionnalitie(s) --> Feature(s) --> Dev --> Main

### Main Branch

#### Use of the branch

The main branch will only host complete and versionned build of the project, every push of this branch correspond to a new version of the software coming out, hence push on main branch is forbidden and can only be done with a pull request from the dev branch after the version requirements are met.
While this project is still incubated in EPITECH, every push on this branch also create a push on the EPITECH repository.

#### PR requirements

A pull request can only be validated after all the associated have been tested and that the code has been crossreviewed by the team and :

- Performance tests coompletion
- Build verification
- (manual) E2E tests

### Dev Branch

#### Use of the branch

The dev branch is the center of the continuous integration, every feature that has been completed on the project will be merged here until a complete version can be made and merged into main.
Push are also forbidden on the dev branch, but a branch can be created directly from dev and not from the associated feature if a major bug arise.

#### PR requirements

A pull request toward ``dev`` can only be merged after :

- Completion of all Integration tests
- Build verification
- Performance tests
- Review by at least one but preferably two devs working on a different feature

### Feature Branch

#### Use of the branch

This branche compiles all the work done on a specific feature during a sprint, multiple devs will work on it together but not the whole team. The common situation is to separate this branch into individual task (called functionnalities) that developpers can work on and then merged into this branch.
Push are allowed on this branch but not recommanded.

#### PR requirements

A pull request toward a ``feature`` can only be merged after :

- Completion of all Unit tests
- Completion of all Integration tests
- Review by a developper working on the same feature

### Functionnality Branch

#### Use of the branch

This branch contains the work of the current task of the developpers, it is the principal branch they work on until they complete and polish their task. As such this task allows push and commit freely.

#### PR requirements

There's no requirements for those branches as they can be managed at goodwill by the developpers, and most of them won't have sub-branches that will create the need for a pull request.

## CI/CD Pipeline

### CI Triggers
The pipeline will be used during those key developpement steps to launch different tests.

- Push inside a branch a functionnality or feature branch
	- Unit tests
	- Check if critical or unnecessary files are pushed
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

On top of that the pipeline will handle the compilation and build of the project as part of the Build verification, thanks to the **Unity Cloud Build**.

### How to deploy

With every release, a release note will get out with a link to the binary of the project.
For now the deployment will be done here but as the project grows it could be then transfered to itch.io or Steam, as it possible for the latter to link the github action to transfer the new files and binary and the release note on Steam.