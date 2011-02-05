
properties {
    $baseDirectory = resolve-path .
    $buildDirectory = "$baseDirectory\build"
    $stageDirectory = "$buildDirectory.staged"

    $solutionFilepath = "$baseDirectory\src\RavenDBMembership.sln"
}


#task default -depends Build,UnitTests,Configure,FullTests
task default -depends Build,Configure,FullTests

task Verify40 {
	if( (ls "$env:windir\Microsoft.NET\Framework\v4.0*") -eq $null ) {
		throw "Building RavenDBMembership requires .NET 4.0, which doesn't appear to be installed on this machine"
	}
}


task Clean {
    if (test-path $buildDirectory) {
        remove-item $buildDirectory -force -recurse 
    }
}

task Build -depends Verify40, Clean {

	$v4_net_version = (ls "$env:windir\Microsoft.NET\Framework\v4.0*").Name
    exec { &"C:\Windows\Microsoft.NET\Framework\$v4_net_version\MSBuild.exe" $solutionFilepath /p:OutDir="$buildDirectory\" }
}

task UnitTests {
    exec { & .\tools\xunit\xunit.console.clr4.exe "$($buildDirectory)\RavenDBMembership.Tests.dll" }
}

task Configure {
}

task FullTests {
    exec { & .\tools\NUnit\net-2.0\nunit-console.exe "$($buildDirectory)\RavenDBMembership.IntegrationTests.dll" }
}
