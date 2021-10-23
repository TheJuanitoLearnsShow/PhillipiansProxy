// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open System.IO.Pipes
open FSharp.Control.Tasks.V2
open System.IO
open System.Diagnostics


let predictorFolderPath = @"E:\OneDrive\sources\PhillipiansProxy\JS\out\"
let predictorPath = @"prediction.js"
    
let StartReadingAsync(pipeReader: AnonymousPipeServerStream) =
    task {
        try
            use sr = new StreamReader(pipeReader);
            
            // This method should get a CancellationToken so we use that instead of a while true.
            // But this will work now.
            while (true) do
            
                let! message = sr.ReadLineAsync();
    
                if (message <> null) then
                    Console.WriteLine(message);
        with ex -> 
            Console.WriteLine(ex);
    }
[<EntryPoint>]
let main argv =
    // We are going to create two pipes, one writer and one reader.
    let pipeWriter = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
    let pipeReader = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);

    // We create a child process passing the pipes handles as string.
    let client = new Process();

    client.StartInfo.FileName <- "node";
    client.StartInfo.Arguments <- predictorFolderPath + predictorPath + " " + pipeWriter.GetClientHandleAsString() + " " + pipeReader.GetClientHandleAsString();
    client.StartInfo.UseShellExecute <- false;
    client.StartInfo.WorkingDirectory <- predictorFolderPath
    let process = client.Start()
      
    // If microsoft docs tells me to call this method I will.
    pipeWriter.DisposeLocalCopyOfClientHandle();
    pipeReader.DisposeLocalCopyOfClientHandle();

    // We start listening to messages
    let _ = StartReadingAsync(pipeReader);

    // We create a stream writer, and we will write messages on that stream.
    use sw = new StreamWriter(pipeWriter)
    sw.AutoFlush <- true

    let filename = "E:\\test\\s1.JPG"
    let imgBytes =  File.ReadAllBytes filename

    sw.Write(imgBytes)
    let mutable message = Console.ReadLine();
    
    while (message <> "exit") do
        sw.Write(imgBytes)
        message <- Console.ReadLine();
    
    client.Close();
    0 // return an integer exit code