using System;
using System.Diagnostics;
using System.Threading;

/// This example shows how to define a process and start it.
/// Check here: https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.process?view=netcore-3.1


namespace Exercise
{
    public class ProcessCreation
    {
        public virtual void createProcess()
        {
            // Todo: Create an object from ProcessStartInfo

            // Todo: Provide the path and the name of your executable file
            //prInfo.CreateNoWindow = false; // This means start the process in a new window
            //prInfo.UseShellExecute = false;

            // Implement your code here ...
            ProcessStartInfo procInfo = new ProcessStartInfo();
            procInfo.FileName = "C:/Users/Thijmen/Desktop/customer-journey-hhsk.pdf";
            procInfo.Arguments = "-all";
            procInfo.CreateNoWindow = false;
            procInfo.UseShellExecute = false;

            Console.WriteLine("gonna execute thingy: {0}, {1}", procInfo.FileName, procInfo.Arguments);

            try
            {
                // Todo: Start your process and then wait for its exit
                // Implement your code here
                using (Process p = Process.Start(procInfo)) {
                    Thread.Sleep(100);
                    Console.WriteLine("sleeping rn; check back l8er");
                    p.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }

        }
    }
}
