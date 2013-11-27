The source code for our project can be found at https://github.com/ShaurinD/STATS.
The application has two executables that need to be run, one for pupil tracking and one for the keyboard.

Keyboard:
Download the Dynamic Keyboard and open in Visual Studio. Open the ACTB project solution. Build the ACTB solution. 
You will not be able to run the application because it is a library. However the build should 
update the executable in either Test/bin/Debug or Test/bin/Release (depending on mode you built in).
Run the updated executable named Test.exe. Changing the Config files will cause bugs in the application, 
they are for the application to read and write data. Any keyboards should be added through
the add keyboards button on the GUI in order to ensure full functionality and proper updates of the config files. 
When running the executable alone, the application will complain about not being able to find files(built-in keyboards) 
that we pre-loaded.  The application will still be useable if you click "ok" and create keyboards yourself with the 
create keyboards button.
