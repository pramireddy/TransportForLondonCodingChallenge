# TransportForLondonCodingChallenge

Instructions to Build and Run the Lab.TfL.RestApi.Clien

  - Tools used: VS 2019, .Net5,C#,xUnit,Moq
  - .Net 5 Console App

How to build the code?

  Step 1:Download the code at https://github.com/pramireddy/TransportForLondonCodingChallenge 

  Step 2: Open the soultion: TransportForLondonCodingChallenge.sln in Visual Studio 2019.

  TransportForLondonCodingChallenge.sln contains:

    - Lab.TfL.Dtos - .Net 5 Class Library
    - Lab.TfL.RestApi.Client - .Net 5 Console Application
    - LLab.TfL.RestApi.Client.UnitTests - .Net 5 xUnit Test Library

How to run the TfRestApiClient:

Step 1: Update "TfLApiSettings" in appsettings.json file:

example:

  "TfLApiSettings": {
    "ApiBaseAddress": "https://api.tfl.gov.uk",
    "RoadEndpoint": "/road",
    "ApiId": "Test@Test.com",
    "ApiKey": "123456789"
  }

Step 2: Run the output at Windows Command Prompt or Windows PowerShell

  <application directory>\TransportForLondonCodingChallenge\Lab.TfL.RestApi.Client\bin\Release\net5.0> dotnet .\Lab.TfL.RestApi.Client.dll RoadID

 
  Example::
    PS C:\Users\prasa\source\repos\TransportForLondonCodingChallenge\Lab.TfL.RestApi.Client\bin\Release\net5.0> dotnet .\Lab.TfL.RestApi.Client.dll A2,A406

  Output:

  The status of the A2 is as follows
           Road status is Serious
           Road status Description is Serious Delays

  The status of the North Circular (A406) is as follows
           Road status is Good
           Road status Description is No Exceptional Delays

How to run unit tests? 

  Visual Studio ==> Test ==> Test Explorer or by using xUnit Test Runner
  
  
