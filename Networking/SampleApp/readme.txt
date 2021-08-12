- there are 2 entities:
    1 Operator
	2 Service

- in Service there are 2 entities:
	1 service - common frame / base frame
	2 core - useful logic / main logic inside a frame
	- main core logic should extend IRunnable

- it is important to have appsettings.json
<Content Include="appsettings.json"> - copy it to bin folder