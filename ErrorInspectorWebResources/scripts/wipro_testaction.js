 function generateDemoException()
        {
            try
            {
                //your custom business logic here                
                //raise intentional exception

                var a = nonExsitingFunction();
            }
            catch (error)
            {
                //your custom exception handling here  
                //log exception. This is the only line of code required in your custom code 
                wipro_Sdk.logException(error, wipro_Sdk.PriorityLevel.HIGH );
                
            }
        }