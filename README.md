# Make IIncrementalGenerator to use cache 


1. For Rider:<br>
`Settings/Prefs` -> Search `Roslyn` -> `Enable Roslyn analysers`


2. Rebuild solution


3. For Rider:<br>
`File` -> `Invalidate caches...`


4. start logger in terminal:<br>
   `> cd Logger`<br>
   `> dotnet run`<br>


5. in `Test\ITarget.cs` try to rename something

Enjoy source generators :)