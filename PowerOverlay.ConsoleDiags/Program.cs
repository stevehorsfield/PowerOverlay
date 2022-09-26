using PowerOverlay.ConsoleDiags.Native;

try
{
    Console.WriteLine("Beginning audio device interface diagnostics...");

    Console.WriteLine("   controller = new MultimediaAudioInterop()");
    var controller = new MultimediaAudioInterop();

    Console.WriteLine("   endpoints = controller.EnumerateAudioEndpoints()");
    var endpoints = controller.EnumerateAudioEndpoints();

    if (endpoints == null) Console.WriteLine("Error: endpoints = null");
    else
    {
        Console.WriteLine("   foreach (var endpoint in endpoints)...");
        foreach (var endpoint in endpoints)
        {
            Console.WriteLine("   -- loop iteration");
            Console.WriteLine($"     [ {endpoint.EndpointFriendlyName} / {endpoint.InterfaceFriendlyName} / {endpoint.EndpointID} ]");
        }
    }

    Console.WriteLine("   defaults = controller.EnumerateDefaultAudioEndpoints()");
    var defaults = controller.EnumerateDefaultAudioEndpoints();
    if (defaults == null) Console.WriteLine("Error: defaults = null");
    else
    {
        Console.WriteLine("   foreach (var d in defaults)...");
        foreach (var d in defaults)
        {
            Console.WriteLine("   -- loop iteration");
            Console.WriteLine($"     [ {d.EndpointID} / {d.Direction} / {d.Role} ]");
        }
    }

    Console.WriteLine("done");
}
catch (Exception e)
{
    Console.WriteLine($"Exception: {e.Message}\nStack trace: {e.StackTrace}");
}

Console.ReadLine();