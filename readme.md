# CPU-Simulator using Windows Forms

This project provides a Windows Forms application that demonstrates common CPU scheduling algorithms through an interactive graphical interface. Each algorithm prompts for basic input and displays the resulting waiting or turnaround times using message boxes and on-screen tables. Additionally metrics are displayed for Average Waiting Time, Average Turnaround Time, CPU Utilization, Throughput, and Response Time. As well as the ability to export the results in a csv file.

**Fork maintained by Ryan Moskovciak** Forked From Chris Regan - Original creator: Francis (used with permission)

## Project status

The simulator is functional but still a work in progress. Currently the following scheduling strategies are available:

| Algorithm | Method | Notes |
|-----------|--------|-------|
| First Come First Serve | `RunFirstComeFirstServe` | Processes are executed in order of arrival. |
| Shortest Job First | `RunShortestJobFirst` | Jobs are sorted by burst time before execution. |
| Priority Scheduling | `RunPriorityScheduling` | User supplies a priority value for each job. |
| Round Robin | `RunRoundRobin` | Requires a quantum time parameter. |
| Shortest Remaining TIme First | `RunSRTFAlgorithm` | Processes are executed based on shortest burst remaining. If new arrivals are short they are executed.
| Multi-Level Feedback Queue | `RunMLFQAlgoritm` | Processes are added to a primary priority queue on arrival. The processes are demoted based on completion of time slice until burst time has been completed.

**The initial creater used an Algorithm.cs to build the algorithms. However the newest version that I forked did not utilize the Algorithm.cs and implemented the algorithms directly in CpuSchedulerForm.cs

## Requirements

- Windows operating system
- .NET 8.0 SDK or newer
- Visual Studio 2022 or VS Code with C# extensions

## How to run

### Using Visual Studio

1. Clone the repository:

   ```bash
   git clone git@github.com:ryanMHub/CS-3502-CPU-Sim-Project-StartingPoint.git
   ```

2. Open `CpuScheduler.sln` in Visual Studio 2022
3. Press F5 to build and run the application

### Using VS Code

1. Clone the repository:

   ```bash
   git clone git@github.com:ryanMHub/CS-3502-CPU-Sim-Project-StartingPoint.git
   ```

2. Install the C# Dev Kit extension in VS Code

3. Open the project folder in VS Code

4. **Option A - Using the Debugger (Recommended):**
   - Press **F5** or go to Run & Debug panel
   - Select ".NET Core Launch (console)" configuration
   - This will build and launch the Windows Forms app with debugging support

5. **Option B - Using Terminal (May have termination issues):**

   ```bash
   dotnet build
   dotnet run --project CpuScheduler/CpuScheduler.csproj
   ```

   **Note:** Windows Forms apps may not terminate cleanly in VS Code's integrated terminal

6. **Option C - Run the Built Executable Directly:**

   ```bash
   dotnet build
   # Then navigate to: CpuScheduler/bin/Debug/net8.0-windows/CpuScheduler.exe
   # Double-click the .exe file or run from command prompt
   ```

### Using .NET CLI

From the project root directory:

```bash
# Build the project
dotnet build

# Run the application
dotnet run --project CpuScheduler/CpuScheduler.csproj
```

## Usage

1. Enter the desired number of processes
2. Choose a scheduling algorithm from the interface
3. The app will prompt for additional values as needed (burst time, priority, quantum time, etc.)
4. View the results in the display table showing waiting times and turnaround times

### License

This project is licensed under the terms of the [MIT license](LICENSE.txt).
