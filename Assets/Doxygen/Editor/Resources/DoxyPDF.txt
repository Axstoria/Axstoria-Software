@echo off
REM %1 Documentation folder

@CALL %2 %1\DeveloperDoxyfile
@CALL %2 %1\UserDoxyfile

REM User PDF  
cd UserPDF\latex
CALL make
copy refman.pdf %1\UserDocumentation.pdf
cd..
cd..

REM Developers PDF
cd DeveloperPDF\latex
CALL make
copy refman.pdf %1\DeveloperDocumentation.pdf
cd..
cd..