Welcome to NAntContrib

In NAntContrib you will find many cool things.
/tasks
/Tools/
And build files to create MSI distributions.

How to build.

1) get and build NAnt from cvs. http://sourceforge.net/cvs/?group_id=31650

2) run NAntContrib.build referencing the version of NAnt just built.

nant -D:nant.dir=h:\cvs\nant\build\nant-0.8.4-debug -f:NAntContrib.build

nant.dir should point to the build directory just above bin.


