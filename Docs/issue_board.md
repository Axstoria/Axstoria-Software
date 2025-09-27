# Issue Board process

## Issue type, Feature and Task

* __Feature__ is a general issue with multiple developpers assigned
* It countains multiple sub_issues called __Task__ that are assigned to only one developper assigned (in most case)
* __Bug__ is an exceptionnal issue that is created when a problem is detected by the comunity on a __Feature__

## Issue Norm

* Every issue must have a coherent description explicating the goal of the issue and the work that has to be accomplished. It must also countains the link to every issues and PR that is related to it (especially in the case of a __Bug__)
* Every issue must use the correct labels and be linked to/part of the project __Issue Board__, with the priority, size, type, status correctly filled. The __Features__ must also have the date corresponding to the sprint they are part of.
* After the start of each sprint and the creation of the branches, the issues must be linked to their corresponding branches. In the case of a __Feature__ it is to be done at the end of the sprint start meeting by the president of the reunion, and for the __Tasks__ it is to be done by the person assigned to the task when they start it.That way when a PR is closed it should automatically update the issue board and close the linked issues.
* It is imperative to update the issue board at every step of your issue in order to keep everyone well aware of the advancement of the sprint.

## Priority

There are levels of priorities for a feature :

* __P0__ : To do urgently, if we have to stop the task that we are currently working on then we will, if possible a crisis meeting will be held to organize ourselves in order to solve this situation.
* __P1__ : Important issue, it is a feature or a technical task that we need in priority or that we wish to present for the next version/beta.
* __P2__ : Classic issue to complete, a task like the functionnality of a button or documenation to write.
* __P3__ : Optionnal issue, if there time to spare only.

## Size

The size of an issue correspond the time it needs when done by a solo developper.

* __XS__ : <= Half a day
* __S__ : <= 1 day
* __M__ : <= 3 days
* __L__ : <= 1 week
* __XL__ : <= 3 weeks
* __XXL__ : 1 sprint *used mainly in the case of a feature that all 6 developpers needs to work on*
A __Feature__ is usually graded to be the size above the biggest __Task__ it countains(or an equal value to that __Task__ if there's a big size differential between the tasks).

## Labels

* __Documentation__ :  Improvements or additions to the documentation
* __UI/UX__ : Improvement or additions to the UI/UX implementation
* __Network__ : Improvement or additions to the network implementation
* __CI/CD__ : Improvement or additions to the CI/CD implementation
* __Architecture__ : Improvement or additions to the architecture of the code
* __Backend Game__ : Improvement or additions to the functionalities of the game
* __Duplicate__ : This issue or pull request already exists
* __Distribution__ : Improvement or additions to the distribution page
* *more label can be added in the feature if the need arise*
The labels have to be applied in priority to the __Tasks__ and then the __Features__ will be composed of the main labels of the __Tasks__.

## Status

* __Backlog__ : Every issues created in advance, they are to be developped at an ulterior time.
* __Ready__ : Issue corresponding to the present sprint, one or multiple devs are assigned to them.
* __Feature In Progress__ : Actual sprint's __Feature__ currently in developpement (There shouldn't be more than 3 or 4 feature at a time except in a crisis time)
* __Task In Progress__ : __Task__ currently being developped by a developper (There shouldn't be more than one per developper except crisis time)
* __In Review__ : Tested and completed issue, this issue is currently being reviewed as part of a Pull Request and needs the input of the PR reviewer to be closes
* __Done__ : Every issue that has been done and integrated.
