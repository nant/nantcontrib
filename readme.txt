NAntContrib

What is it?
-----------
NAntContrib is the project for tasks and tools that haven't made it into the main NAnt distribution 
yet or for whatever reason don't belong there.


How to use NAntContrib tasks in NAnt?
-------------------------------------
In order to use NAntContrib tasks in NAnt, you should use the NAnt's <loadtasks> task.

For example :

<project name="NAntContrib" default"test">
	<target name="test">
		<loadtasks assembly="c:/NAntContrib/bin/NAnt.Contrib.Tasks.dll" />
		...
	</target>
</project>


How to build.
-------------
To build NAntContrib, the following procedure should be followed:

1) Download and extract a binary distribution of NAnt from http://nant.sourceforge.net

2) Change to the NAntContrib directory

3) Run NAntContrib.build using the version of NAnt that you downloaded.

	eg.  c:\NAnt\bin\NAnt.exe -D:nant.dir=c:\NAnt -f:NAntContrib.build

Note: 

These instructions only apply to the source distribution of NAntContrib, as the binary distribution 
contains pre-built assemblies.


Documentation
-------------
Documentation is available in HTML format, in the doc/ directory.
