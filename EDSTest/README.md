# Authorization Code Flow + PKCE DotNet Sample and Test

[![Build Status](https://dev.azure.com/osieng/engineering/_apis/build/status/product-readiness/OCS/Auth_PKCE_DotNet?branchName=master)](https://dev.azure.com/osieng/engineering/_build/latest?definitionId=863&branchName=master)


## Requirements

- .NET Core 3.1 is installed
- Replace the placeholders in the [appsettings](appsettings.json) file with your EDS Port, API Version, Tenant ID, and Namespace ID


## Running the sample

### Prerequisites

- Have EDS installed and running on local machine.

### Using Visual Studio

1. Load the .csproj in this directory
2. Rebuild project
3. Run it

- If you want to see outputs from the program, put a breakpoint at the end of the main method and run in debug mode

4. The outputs tell you which step has completed/ if an error has occurred 

### Using Command Line

- Make sure you have the install location of dotnet added to your path
- Run the following command from the location of this project:

```shell
dotnet run
```

## Running the automated test

### Using Visual Studio

- Load the .csproj from the AuthorizationCodeFlowTest directory above this in Visual Studio
- Rebuild project
- Open Test Explorer and make sure there is one test called EDSTestTests is showing
- Run the test

### Using Command Line

- Make sure you have the install location of dotnet added to your path
- Run the following command from the location of the EDSTest project:

```shell
dotnet test
```

For the main EDS page [ReadMe](https://osisoft.github.io/Edge-Data-Store-Docs/V1/)  
For the main samples page on master [ReadMe](https://github.com/osisoft/OSI-Samples)


## Sample Application Contents

The purpsoe of this sample application is to demonstrate the EDS's compatibility with a filter. This application reads in data from a 
sine wave stream of data (between 1.0 and -1.0) and filters out the values between -0.9 to 0.9. The data that is left is sent to a 
new stream and EDS. This application is an example of how exception reporting can be used with EDS.

Step 1
- Creates the SineWave type using SDS. EDS requires that a timestamp Datetime value is used as the key.

Step 2
- Creates the SineWave stream using SDS

Step 3
- New events are initialized with sine wave data ranging from -1 to 1. This data is sent to the SineWave stream using SDS

Step 4
- Data from the SineWave stream is ingressed and stored in a list of SineWave objects. Since data is encoded using GZIP, some decoding is neccessary.

Step 5
- Creates the FilteredSineWave stream using SDS

Step 6
- The filter is a deadband filter that accepts only data greater than 0.9 or less than -0.9. This portion can be altered to 
be any type of filter. The data is loaded into a new list of SineWave objects and is sent to the FilteredSineWave stream.

Step 7
- The Types and Streams creatied by the application are deleted.


