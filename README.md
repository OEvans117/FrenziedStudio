# FrenziedStudio
## This project simplifies the coding process for multithreaded web automation.

* This project will always be open source / free to download.
* This project is no where near production ready, so please expect this.
* Commits & branches have been done on an old account, so updates are yet to be added.

**Run a FrenziedLang project:**

``var currentProject = new InterpretedProject(currentScript.interpretString);``

``Interpreting.RunProject(currentProject, true);``

**Documentation:**

https://git-34.gitbook.io/frenziedstudio/

**Example of current commands:**

| Command  | Description | Parameter 1  | Parameter 2 |  Values Example
| ------------- | ------------- | ------------- | ------------- | ------------- | 
| chrome  | Browser commands  | start, quit  |   | start, quit  |
| goto  | Visit a website  | URL (string)  |  | https://google.com/ |
| waitelement  | Wait for element | ElementIdentifier | integer | name->password->20 |
| sendkeys  | Type into element | ElementIdentifier | string | name->pass->hi there |
| typeslow  | Type slow into element | ElementIdentifier | string, (int, int) | name->user->bob->50,90 |
| click  | Click an element | ElementIdentifier |  | name->submit |
| scroll  | Scroll element into view | ElementIdentifier |  | classname->test |
| select  | Select option element | ElementIdentifier | SelectType, string | text->august |
| switchframe  | Switch frame | "parent", FrameNumber, ElementIdentifier | | xpath->//iframe[@class='t'] |
| setcookies  | Set session cookies | | | |
| screenshot  | Screenshot page | Path (string) | | C:\Pictures |
