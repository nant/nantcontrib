Welcome to NAntContrib

In NAntContrib you will find many cool things.
/tasks
/Tools/
And build files to create MSI distributions.

How to build.

1) get and build NAnt from cvs. http://sourceforge.net/cvs/?group_id=31650
	- change to the nant directory
	- build using the command: 

		bin\NAnt.exe package

	NOTE: You may need to run vcvars32.bat to set your path correctly to run the unit tests.

2) run NAntContrib.build referencing the version of NAnt just built.
	- change to the NAntContrib directory
	- build using the command

		nant -D:nant.dir=h:\cvs\nant\build\nant-0.8.4-debug -f:NAntContrib.build

	NOTE: nant.dir should point to the build directory from step 1, just above bin.


