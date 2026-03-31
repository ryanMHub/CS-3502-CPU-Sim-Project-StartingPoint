using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using Microsoft.VisualBasic;

namespace CpuScheduler
{
    /// <summary>
    /// Main form for demonstrating CPU scheduling algorithms.
    /// </summary>
    public partial class CpuSchedulerForm : Form
    {
        private DataTable processTable;
        private Random random = new Random();
        private bool isDarkMode = true; // Default to dark mode
        
        // STUDENTS: Configure these limits based on your algorithm performance requirements
        private const int MIN_PROCESS_COUNT = 1;
        private const int MAX_PROCESS_COUNT = 200;
        private const int DEFAULT_PROCESS_COUNT = 3;

        //TODO: fields for result export
        private List<SchedulingResult> lastDisplayedResults = new List<SchedulingResult>();
        private string lastDisplayedAlgorithmName = "";
        private double[] metrics = new double[9];

        /// <summary>
        /// Initializes a new instance of the <see cref="CpuSchedulerForm"/> class.
        /// </summary>
        public CpuSchedulerForm()
        {
            InitializeComponent();
            InitializeProcessTable();
        }

        /// <summary>
        /// Handles welcome page navigation.
        /// </summary>
        private void WelcomeButton_Click(object sender, EventArgs e)
        {
            ShowPanel(welcomePanel);
            sidePanel.Height = btnWelcome.Height;
            sidePanel.Top = btnWelcome.Top;
        }

        /// <summary>
        /// Handles results navigation.
        /// </summary>
        private void DashBoardButton_Click(object sender, EventArgs e)
        {
            ShowPanel(resultsPanel);
            sidePanel.Height = btnDashBoard.Height;
            sidePanel.Top = btnDashBoard.Top;
        }

        /// <summary>
        /// Navigates to the scheduler panel.
        /// </summary>
        private void CpuSchedulerButton_Click(object sender, EventArgs e)
        {
            ShowPanel(schedulerPanel);
            sidePanel.Height = btnCpuScheduler.Height;
            sidePanel.Top = btnCpuScheduler.Top;
        }

        /// <summary>
        /// Handles About page navigation.
        /// </summary>
        private void AboutButton_Click(object sender, EventArgs e)
        {
            ShowPanel(aboutPanel);
            sidePanel.Height = btnAbout.Height;
            sidePanel.Top = btnAbout.Top;
        }

        /// <summary>
        /// Toggles between dark and light mode themes.
        /// </summary>
        private void DarkModeToggle_Click(object sender, EventArgs e)
        {
            isDarkMode = !isDarkMode;
            ApplyTheme();
        }

        /// <summary>
        /// Shows the specified panel and hides all others.
        /// </summary>
        private void ShowPanel(Panel panelToShow)
        {
            welcomePanel.Visible = false;
            schedulerPanel.Visible = false;
            resultsPanel.Visible = false;
            aboutPanel.Visible = false;
            panelToShow.Visible = true;
            panelToShow.BringToFront();
        }

        /// <summary>
        /// Initializes the Welcome panel with introduction and navigation guide.
        /// </summary>
        private void InitializeWelcomeContent()
        {
            welcomeTextBox.Text = @"Welcome to CPU Scheduler Simulator

This educational tool helps CS 3502 students learn and experiment with CPU scheduling algorithms used in operating systems.

GETTING STARTED

Navigate using the sidebar buttons on the left:

🏠 WELCOME
This introduction page explaining the simulator and navigation.

⚙️ SCHEDULER
The main interface where you can:
• Enter the number of processes to simulate
• Choose from 4 scheduling algorithms:
  - FCFS (First Come, First Serve)
  - SJF (Shortest Job First)
  - Priority Scheduling
  - Round Robin
• Run simulations and see immediate feedback

📊 RESULTS
View detailed results from your last algorithm execution:
• Process execution details
• Algorithm-specific information
• Summary statistics
Results persist until you run a new simulation.

📚 ABOUT
Learn about the algorithms:
• How each algorithm works
• When to use each algorithm
• Learning objectives and concepts
• Algorithm characteristics and trade-offs

🔄 RESTART APPLICATION
Reset the simulator to its initial state.

HOW TO USE
1. Click 'Scheduler' to start
2. Enter number of processes (try 3-5 for learning)
3. Click an algorithm button to run simulation
4. View results in the 'Results' section
5. Learn more in the 'About' section
6. Experiment with different algorithms and process counts

Ready to start? Click 'Scheduler' to begin your CPU scheduling exploration!";
        }

        /// <summary>
        /// Initializes the About tab with educational content about CPU scheduling algorithms.
        /// </summary>
        private void InitializeAboutContent()
        {
            aboutTextBox.Text = @"CPU Scheduling Algorithms

This simulator demonstrates four fundamental CPU scheduling algorithms used in operating systems:

FIRST COME, FIRST SERVE (FCFS)
• Non-preemptive algorithm
• Processes are executed in the order they arrive
• Simple to implement but can lead to convoy effect
• Good for batch systems with long processes

SHORTEST JOB FIRST (SJF)
• Non-preemptive algorithm  
• Selects process with shortest burst time first
• Optimal for minimizing average waiting time
• Requires knowledge of process execution times

PRIORITY SCHEDULING
• Can be preemptive or non-preemptive
• Each process has a priority number
• CPU allocated to highest priority process
• May cause starvation of low-priority processes

ROUND ROBIN (RR)
• Preemptive algorithm using time quantum
• Each process gets equal CPU time slices
• Good for time-sharing systems
• Performance depends on quantum size

Learning Objectives:
• Understand how different algorithms handle process scheduling
• Compare algorithm performance and characteristics  
• Explore trade-offs between fairness and efficiency
• Learn when to use each algorithm type

Instructions:
1. Use the Scheduler tab to run algorithms
2. View execution results in the Results tab
3. Experiment with different process counts
4. Compare algorithm behaviors and outcomes";
        }

        /// <summary>
        /// STUDENTS: Helper method to get process data from the DataGrid
        /// Use this in your custom algorithm implementations instead of prompting users
        /// Returns: List of process data (ID, Burst Time, Priority, Arrival Time)
        /// </summary>
        public List<ProcessData> GetProcessDataFromGrid()
        {
            var processList = new List<ProcessData>();
            foreach (DataRow row in processTable.Rows)
            {
                processList.Add(new ProcessData
                {
                    ProcessID = row["Process ID"].ToString(),
                    BurstTime = Convert.ToInt32(row["Burst Time"]),
                    Priority = Convert.ToInt32(row["Priority"]),
                    ArrivalTime = Convert.ToInt32(row["Arrival Time"])
                });
            }
            return processList;
        }

        /// <summary>
        /// STUDENTS: Data structure for process information
        /// Use this when implementing your custom scheduling algorithms
        /// </summary>
        public class ProcessData
        {
            public string ProcessID { get; set; }
            public int BurstTime { get; set; }
            public int Priority { get; set; }
            public int ArrivalTime { get; set; }
        }

        /// <summary>
        /// STUDENTS: Validates process count input with configurable limits
        /// Returns true if valid, false otherwise
        /// </summary>
        private bool IsValidProcessCount(string input, out int processCount)
        {
            if (int.TryParse(input, out processCount))
            {
                return processCount >= MIN_PROCESS_COUNT && processCount <= MAX_PROCESS_COUNT;
            }
            processCount = 0;
            return false;
        }

        /// <summary>
        /// STUDENTS: Example FCFS algorithm implementation using DataGrid data
        /// This replaces the old prompt-based system with direct data access
        /// </summary>
        private List<SchedulingResult> RunFCFSAlgorithm(List<ProcessData> processes)
        {
            var results = new List<SchedulingResult>();
            var currentTime = 0;
            
            // Sort by arrival time for FCFS
            var sortedProcesses = processes.OrderBy(p => p.ArrivalTime).ToList();
            
            foreach (var process in sortedProcesses)
            {
                var startTime = Math.Max(currentTime, process.ArrivalTime);
                var finishTime = startTime + process.BurstTime;
                var waitingTime = startTime - process.ArrivalTime;
                var turnaroundTime = finishTime - process.ArrivalTime;
                
                results.Add(new SchedulingResult
                {
                    ProcessID = process.ProcessID,
                    ArrivalTime = process.ArrivalTime,
                    BurstTime = process.BurstTime,
                    StartTime = startTime,
                    FinishTime = finishTime,
                    WaitingTime = waitingTime,
                    TurnaroundTime = turnaroundTime
                });
                
                currentTime = finishTime;
            }
            
            return results;
        }

        /// <summary>
        /// STUDENTS: SJF algorithm implementation using DataGrid data
        /// Shortest Job First - selects process with minimum burst time
        /// </summary>
        private List<SchedulingResult> RunSJFAlgorithm(List<ProcessData> processes)
        {
            var results = new List<SchedulingResult>();
            var currentTime = 0;
            var remainingProcesses = processes.ToList();
            
            while (remainingProcesses.Count > 0)
            {
                // Get processes that have arrived by current time
                var availableProcesses = remainingProcesses.Where(p => p.ArrivalTime <= currentTime).ToList();
                
                if (availableProcesses.Count == 0)
                {
                    // No process has arrived yet, jump to next arrival time
                    currentTime = remainingProcesses.Min(p => p.ArrivalTime);
                    continue;
                }
                
                // Select process with shortest burst time
                var nextProcess = availableProcesses.OrderBy(p => p.BurstTime).ThenBy(p => p.ArrivalTime).First();
                
                var startTime = Math.Max(currentTime, nextProcess.ArrivalTime);
                var finishTime = startTime + nextProcess.BurstTime;
                var waitingTime = startTime - nextProcess.ArrivalTime;
                var turnaroundTime = finishTime - nextProcess.ArrivalTime;
                
                results.Add(new SchedulingResult
                {
                    ProcessID = nextProcess.ProcessID,
                    ArrivalTime = nextProcess.ArrivalTime,
                    BurstTime = nextProcess.BurstTime,
                    StartTime = startTime,
                    FinishTime = finishTime,
                    WaitingTime = waitingTime,
                    TurnaroundTime = turnaroundTime
                });
                
                currentTime = finishTime;
                remainingProcesses.Remove(nextProcess);
            }
            
            return results.OrderBy(r => r.StartTime).ToList();
        }

        /// <summary>
        /// STUDENTS: Priority algorithm implementation using DataGrid data
        /// Higher priority number = higher priority (1 is lowest, higher numbers are higher priority)
        /// </summary>
        private List<SchedulingResult> RunPriorityAlgorithm(List<ProcessData> processes)
        {
            var results = new List<SchedulingResult>();
            var currentTime = 0;
            var remainingProcesses = processes.ToList();
            
            while (remainingProcesses.Count > 0)
            {
                // Get processes that have arrived by current time
                var availableProcesses = remainingProcesses.Where(p => p.ArrivalTime <= currentTime).ToList();
                
                if (availableProcesses.Count == 0)
                {
                    // No process has arrived yet, jump to next arrival time
                    currentTime = remainingProcesses.Min(p => p.ArrivalTime);
                    continue;
                }
                
                // Select process with highest priority (highest number)
                var nextProcess = availableProcesses.OrderByDescending(p => p.Priority).ThenBy(p => p.ArrivalTime).First();
                
                var startTime = Math.Max(currentTime, nextProcess.ArrivalTime);
                var finishTime = startTime + nextProcess.BurstTime;
                var waitingTime = startTime - nextProcess.ArrivalTime;
                var turnaroundTime = finishTime - nextProcess.ArrivalTime;
                
                results.Add(new SchedulingResult
                {
                    ProcessID = nextProcess.ProcessID,
                    ArrivalTime = nextProcess.ArrivalTime,
                    BurstTime = nextProcess.BurstTime,
                    StartTime = startTime,
                    FinishTime = finishTime,
                    WaitingTime = waitingTime,
                    TurnaroundTime = turnaroundTime
                });
                
                currentTime = finishTime;
                remainingProcesses.Remove(nextProcess);
            }
            
            return results.OrderBy(r => r.StartTime).ToList();
        }

        /// <summary>
        /// STUDENTS: Round Robin algorithm implementation using DataGrid data
        /// Each process gets a time quantum, then cycles to next process
        /// </summary>
        private List<SchedulingResult> RunRoundRobinAlgorithm(List<ProcessData> processes, int quantumTime = 4)
        {
            var results = new List<SchedulingResult>();
            var currentTime = 0;
            var processQueue = new Queue<ProcessData>();
            var processResults = new Dictionary<string, SchedulingResult>();
            var remainingBurstTimes = new Dictionary<string, int>();
            
            // Initialize remaining burst times and results
            foreach (var process in processes)
            {
                remainingBurstTimes[process.ProcessID] = process.BurstTime;
                processResults[process.ProcessID] = new SchedulingResult
                {
                    ProcessID = process.ProcessID,
                    ArrivalTime = process.ArrivalTime,
                    BurstTime = process.BurstTime,
                    StartTime = -1, // Will be set on first execution
                    FinishTime = 0,
                    WaitingTime = 0,
                    TurnaroundTime = 0
                };
            }
            
            // Add processes that arrive at time 0
            foreach (var process in processes.Where(p => p.ArrivalTime <= currentTime).OrderBy(p => p.ArrivalTime))
            {
                processQueue.Enqueue(process);
            }
            
            var processesNotInQueue = processes.Where(p => p.ArrivalTime > currentTime).OrderBy(p => p.ArrivalTime).ToList();
            
            while (processQueue.Count > 0 || processesNotInQueue.Count > 0)
            {
                // Add any processes that have now arrived
                while (processesNotInQueue.Count > 0 && processesNotInQueue[0].ArrivalTime <= currentTime)
                {
                    processQueue.Enqueue(processesNotInQueue[0]);
                    processesNotInQueue.RemoveAt(0);
                }
                
                if (processQueue.Count == 0)
                {
                    // No processes in queue, jump to next arrival
                    currentTime = processesNotInQueue[0].ArrivalTime;
                    continue;
                }
                
                var currentProcess = processQueue.Dequeue();
                var result = processResults[currentProcess.ProcessID];
                
                // Set start time if this is the first execution
                if (result.StartTime == -1)
                {
                    result.StartTime = currentTime;
                }
                
                // Execute for quantum time or remaining burst time, whichever is smaller
                var executionTime = Math.Min(quantumTime, remainingBurstTimes[currentProcess.ProcessID]);
                currentTime += executionTime;
                remainingBurstTimes[currentProcess.ProcessID] -= executionTime;
                
                // Add any processes that arrived during this execution
                while (processesNotInQueue.Count > 0 && processesNotInQueue[0].ArrivalTime <= currentTime)
                {
                    processQueue.Enqueue(processesNotInQueue[0]);
                    processesNotInQueue.RemoveAt(0);
                }
                
                // Check if process is completed
                if (remainingBurstTimes[currentProcess.ProcessID] == 0)
                {
                    result.FinishTime = currentTime;
                    result.TurnaroundTime = result.FinishTime - result.ArrivalTime;
                    result.WaitingTime = result.TurnaroundTime - result.BurstTime;
                }
                else
                {
                    // Process not completed, add back to queue
                    processQueue.Enqueue(currentProcess);
                }
            }
            
            return processResults.Values.OrderBy(r => r.StartTime).ToList();
        }

        //TODO: This is the first Algo I added
        /// <summary>
        /// Professor: SRTF algorithm implementation using DataGrid data
        /// SRTF Stands for Shortest Remaining Time First. This algo builds on 
        /// SJF, but it is preemptive, if a new process arrives thats and it's 
        /// total time needed is shorter than the current process. The current
        /// process will go into a ready queue. And then the shorter process 
        /// will execute.
        /// </summary>
        private List<SchedulingResult> RunSRTFAlgorithm(List<ProcessData> processes)
        {
            //Declare and instantiate datastructures to track processes
            var processResults = new Dictionary<string, SchedulingResult>(); //process final output row
            var remainBurstTimes = new Dictionary<string, int>(); //required time for each process to finish
            var completed = new HashSet<string>(); //completed processes

            //counters & clock
            int currTime = 0; //cpu clock
            int completedCount = 0; //quantity of processes completed
            int totalProcesses = processes.Count; //Starting quantity of processes

            //Initialize all processes
            foreach (var process in processes)
            {
                remainBurstTimes[process.ProcessID] = process.BurstTime; //set initial burst time

                //initialize the starting SchedulingResult for each process
                processResults[process.ProcessID] = new SchedulingResult
                {
                    ProcessID = process.ProcessID,
                    ArrivalTime = process.ArrivalTime,
                    BurstTime = process.BurstTime,
                    StartTime = -1, //Flag signals unstarted process
                    FinishTime = 0,
                    WaitingTime = 0,
                    TurnaroundTime = 0
                };

            }

            //start the time based on the earliest arrival time
            if(processes.Count > 0)
            {
                currTime = processes.Min(p => p.ArrivalTime);
            }

            //Primary Loop -> Continue until all processes completed
            while(completedCount < totalProcesses)
            {
                //Most important part gathers all processes that are available for processesing
                //This is the primary action of this algorithm
                var availProcesses = processes
                    .Where(p => p.ArrivalTime <= currTime //make sure the arrival time of the process is less than or equal to current sim clock
                        && !completed.Contains(p.ProcessID) //make sure it has not been added to the completed set
                        && remainBurstTimes[p.ProcessID] > 0) //make sure it has a burst time
                    .OrderBy(p => remainBurstTimes[p.ProcessID]) //Sort by Burst value
                    .ThenBy(p => p.ArrivalTime) //arrival time
                    .ThenBy(p => p.ProcessID) //and process id
                    .ToList(); //lastly create a list
                
                //if the list created above is empty increase the sim clock and start loop over
                if(availProcesses.Count == 0)
                {
                    currTime++;
                    continue;
                }

                var currProcess = availProcesses.First(); //pop the first value from list which is shortest remaining time
                var result = processResults[currProcess.ProcessID]; //optain process records

                //if the process has not been started yet set it to currTime.
                if(result.StartTime == -1)
                {
                    result.StartTime = currTime;
                }

                //reduce remaining burst time of the current process and increase sim clock
                remainBurstTimes[currProcess.ProcessID]--;
                currTime++;

                //if the current process has expended it's burst time finish process.
                //Set all important values in the SchedulingResult of the process.
                if(remainBurstTimes[currProcess.ProcessID] == 0)
                {
                    result.FinishTime = currTime;
                    result.TurnaroundTime = result.FinishTime - result.ArrivalTime;
                    result.WaitingTime = result.TurnaroundTime - result.BurstTime;

                    completed.Add(currProcess.ProcessID);
                    completedCount++;
                }

            }
            
            //return list of results
            return processResults.Values
                .OrderBy(r => r.StartTime)
                .ToList();            
        }

        //TODO: This is the first Algo I added
        /// <summary>
        /// Professor: The implementation below represents a 3 - level Multi-levl Feedback Queue (MLFQ)
        /// algorithm. This algorithm essential combines two of the algrithms we started with. One queue will
        /// represent the Roud Robin at a quantum of 2, a Round Robin with the quantum of 4, and an FCFS style
        /// which will run to completion.
        /// </summary>
        private List<SchedulingResult> RunMLFQAlgoritm(List<ProcessData> processes)
        {
            var processResults = new Dictionary<string, SchedulingResult>(); //store the scheduling results for each process
            var remainBurstTime = new Dictionary<string, int>(); //Stores the remaing burst time for each process
            var completedProcesses = new HashSet<string>(); //Stores all process that have completed their burst
            var enqueuedProcesses = new HashSet<string>(); //Helps prevent repeated addition to initial queue

            Queue<ProcessData> RR1 = new(); //quick response for new jobs
            Queue<ProcessData> RR2 = new(); //for longer jobs that didn't finish
            Queue<ProcessData> FCFS = new(); //low priority, CPU heavy tasks

            //SEt quantum values
            const int q1Quant = 2;
            const int q2Quant = 4;

            //set timers and clocks
            int currTime = 0; //simulated cpu clock
            int completedCount = 0; //count of process completed
            int totalProcesses = processes.Count; //Starting quantity of processes

            foreach(var process in processes)
            {
                remainBurstTime[process.ProcessID] = process.BurstTime; //initialize each process

                //initialize scheduling result details
                processResults[process.ProcessID] = new SchedulingResult
                {
                    ProcessID = process.ProcessID,
                    ArrivalTime = process.ArrivalTime,
                    BurstTime = process.BurstTime,
                    StartTime = -1,
                    FinishTime = 0,
                    WaitingTime = 0,
                    TurnaroundTime = 0
                };
            }

            //start the time based on the earliest arrival time
            if(processes.Count > 0)
            {
                currTime = processes.Min(p => p.ArrivalTime);                
            }

            while(completedCount < totalProcesses)
            {
                //Each new item must be added to RR1 to start
                foreach(var process in processes
                    .Where(p => p.ArrivalTime <= currTime
                        && !completedProcesses.Contains(p.ProcessID) //make sure it is not already completed
                        && !enqueuedProcesses.Contains(p.ProcessID))) //make sure it has not already been added to RR1
                {
                    RR1.Enqueue(process);
                    enqueuedProcesses.Add(process.ProcessID);
                }

                //Check if all queues are empty
                if(RR1.Count == 0 && RR2.Count == 0 && FCFS.Count == 0)
                {
                    //Skip to the next available time slot
                    var next = processes
                        .Where(p => !completedProcesses.Contains(p.ProcessID)
                            && !enqueuedProcesses.Contains(p.ProcessID))
                        .Min(p => p.ArrivalTime); //minimum available time slot that has not already been pulled from the dataset
                    currTime = next; //jump the time simulator
                    continue;
                }

                ProcessData currentProcess; //current process working
                SchedulingResult result; //The scheduling result for the current process
                int timeSlice;  //time alloted for the process to run during this pass
                int currentLevel;  //The queue level the process came from

                //Starting with the highest priority process. Begin with
                //RR1 and work your way down. Which ever queue is the first 
                //available process will set the process, level, and timeSlice allotted
                if(RR1.Count > 0)
                {
                    currentProcess = RR1.Dequeue();
                    currentLevel = 1;
                    timeSlice = q1Quant;
                } 
                else if (RR2.Count > 0)
                {
                    currentProcess = RR2.Dequeue();
                    currentLevel = 2;
                    timeSlice = q2Quant;    
                } 
                else
                {
                    currentProcess = FCFS.Dequeue();
                    currentLevel = 3;
                    timeSlice = remainBurstTime[currentProcess.ProcessID];
                }

                //call for the processResults record for the current process
                result = processResults[currentProcess.ProcessID];

                //if this is the first time the process has been seen
                //set StartTime to the current time.
                if(result.StartTime == -1)
                {
                    result.StartTime = currTime;
                }
                
                //Determine the amount of time that the process will run for this turn
                int exeTime = Math.Min(timeSlice, remainBurstTime[currentProcess.ProcessID]);

                //Run the cycle for the process
                for( ; 0 < exeTime; exeTime--)
                {
                    remainBurstTime[currentProcess.ProcessID]--; //reduce the process burst time
                    currTime++; //increase sim clock

                    //Add all new process that arrived during the clock cycle
                    foreach(var process in processes
                        .Where(p => p.ArrivalTime <= currTime
                            && !completedProcesses.Contains(p.ProcessID)
                            && !enqueuedProcesses.Contains(p.ProcessID)))
                    {
                        RR1.Enqueue(process);
                        enqueuedProcesses.Add(process.ProcessID);
                    }

                    //if the current process's burst time has been expended break loop
                    if(remainBurstTime[currentProcess.ProcessID] == 0)
                    {
                        break;
                    }

                    if(currentLevel > 1 && RR1.Count > 0)
                    {
                        break;
                    }
                }

                //if process completed set finished time
                if(remainBurstTime[currentProcess.ProcessID] == 0)
                {
                    result.FinishTime = currTime;
                    result.TurnaroundTime = result.FinishTime - result.ArrivalTime;
                    result.WaitingTime = result.TurnaroundTime - result.BurstTime;

                    completedProcesses.Add(currentProcess.ProcessID);
                    completedCount++;
                    continue;
                }

                //If the process has not finished rerack it or demote it
                if(currentLevel == 1)
                {
                    RR2.Enqueue(currentProcess);
                } 
                else if (currentLevel == 2 && exeTime > 0)
                {
                    RR2.Enqueue(currentProcess);    
                }
                else
                {
                    FCFS.Enqueue(currentProcess);
                }
            }

            //return the list of SchedulingResults
            return [.. processResults.Values.OrderBy(r => r.StartTime)];            
        }

        /// <summary>
        /// STUDENTS: Data structure for algorithm results
        /// Use this to store and display scheduling algorithm outcomes
        /// </summary>
        public class SchedulingResult
        {
            public string ProcessID { get; set; }
            public int ArrivalTime { get; set; }
            public int BurstTime { get; set; }
            public int StartTime { get; set; }
            public int FinishTime { get; set; }
            public int WaitingTime { get; set; }
            public int TurnaroundTime { get; set; }
        }

        /// <summary>
        /// STUDENTS: Displays scheduling results in a formatted table
        /// Use this method to show your algorithm results consistently
        /// </summary>
        private void DisplaySchedulingResults(List<SchedulingResult> results, string algorithmName)
        {
            //save the results data and the name of algorithm
            lastDisplayedAlgorithmName = algorithmName;
            lastDisplayedResults = results;

            listView1.Clear();
            listView1.View = View.Details;

            // Set up columns for detailed results
            listView1.Columns.Add("Process ID", 100, HorizontalAlignment.Center);
            listView1.Columns.Add("Arrival", 80, HorizontalAlignment.Center);
            listView1.Columns.Add("Burst", 80, HorizontalAlignment.Center);
            listView1.Columns.Add("Start", 80, HorizontalAlignment.Center);
            listView1.Columns.Add("Finish", 80, HorizontalAlignment.Center);
            listView1.Columns.Add("Waiting", 80, HorizontalAlignment.Center);
            listView1.Columns.Add("Turnaround", 90, HorizontalAlignment.Center);

            // Add process results
            foreach (var result in results)
            {
                var item = new ListViewItem(result.ProcessID);
                item.SubItems.Add(result.ArrivalTime.ToString());
                item.SubItems.Add(result.BurstTime.ToString());
                item.SubItems.Add(result.StartTime.ToString());
                item.SubItems.Add(result.FinishTime.ToString());
                item.SubItems.Add(result.WaitingTime.ToString());
                item.SubItems.Add(result.TurnaroundTime.ToString());
                listView1.Items.Add(item);
            }                     

            // TODO: STUDENTS - Add performance metrics calculation and display here
            // Required metrics for your project report:
            // 1. Average Waiting Time (AWT) - sum of all waiting times / number of processes
            // 2. Average Turnaround Time (ATT) - sum of all turnaround times / number of processes  
            // 3. CPU Utilization (%) - (total burst time / total time) * 100
            // 4. Throughput (processes/second) - number of processes / total time
            // 5. Response Time (RT) [Optional] - time from arrival to first execution
            // Display these metrics in the results view for comparison between algorithms

             // Add summary statistics
            metrics[0] = results.Average(r => r.WaitingTime);
            metrics[1] = results.Average(r => r.TurnaroundTime);
            
            //Calculate CPU Utilization
            metrics[5] = results.Min(r => r.ArrivalTime); //firstFinish
            metrics[6] = results.Max(r => r.FinishTime); //lastfinish
            metrics[7] = metrics[6] - metrics[5]; //lastFinish - firstFinish = totaltime
            metrics[8] = results.Sum(r => r.BurstTime); //totalBurst
            metrics[2] = metrics[7] > 0 ? (double)metrics[8] / metrics[7] * 100 : 0; //CPU utilization

            //Calculate Throughput
            metrics[3] = metrics[7] > 0 ? (double)results.Count / metrics[7] : 0;

            //Calculate Average Response Time
            metrics[4] = results.Average(r => r.StartTime - r.ArrivalTime);
            
            var summaryItem = new ListViewItem("SUMMARY");
            summaryItem.SubItems.Add(algorithmName);
            summaryItem.SubItems.Add($"{results.Count} processes");
            summaryItem.SubItems.Add($"Avg Wait: {metrics[0]:F1}");
            summaryItem.SubItems.Add($"Avg Turn: {metrics[1]:F1}");
            summaryItem.SubItems.Add($"CPU Utilization: {metrics[2]:F1}%");
            summaryItem.SubItems.Add($"Throughput: {metrics[3]:F3} proccesor/unit");
            summaryItem.SubItems.Add($"Response Time: {metrics[4]:F1}");
            listView1.Items.Add(summaryItem);                        
        }

        // TODO: STUDENTS - Add CSV export functionality for results data
        // Create a "Export Results" button in the results panel to save:
        // - Individual process results (what's shown in listView1)
        // - Performance metrics summary for each algorithm tested
        // Reference the SaveData_Click() method above to learn CSV file handling
        // This will help you create tables/charts for your project report

        /// <summary>
        /// Professor: Saves SchedulingResults data and benchmark results to CSV file for external editing or backup
        /// This allows you to prepare result data in Excel/CSV editors
        /// </summary>
        private void ResultData_Click(object sender, EventArgs e)
        {
            if (processTable.Rows.Count == 0)
            {
                MessageBox.Show("No result data to export. Please run an algo first.", 
                    "No REsults", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveDialog.DefaultExt = "csv";
                saveDialog.FileName = $"{lastDisplayedAlgorithmName.Replace(" ", "_")}_Results.csv";
                saveDialog.Title = "Export Scheduling Results";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var writer = new System.IO.StreamWriter(saveDialog.FileName))
                        {

                            //write algo name
                            writer.WriteLine("Algorithm: " + lastDisplayedAlgorithmName);
                            writer.WriteLine();

                            // Write header
                            writer.WriteLine("Process ID,Arrival,Burst,Start,Finish,Waiting,Turnaround");
                            
                            // Write data rows
                            foreach (var result in lastDisplayedResults)
                            {
                                writer.WriteLine(
                                    $"{result.ProcessID}," +
                                    $"{result.ArrivalTime}," +
                                    $"{result.BurstTime}," +
                                    $"{result.StartTime}," +
                                    $"{result.FinishTime}," +
                                    $"{result.WaitingTime}," +
                                    $"{result.TurnaroundTime}");
                            }

                            writer.WriteLine();

                            //writer metrics to file
                            writer.WriteLine("Metric,Value");
                            writer.WriteLine($"Process Count,{lastDisplayedResults.Count}");
                            writer.WriteLine($"Average Waiting Time,{metrics[0]:F2}");
                            writer.WriteLine($"Average Turnaround Time,{metrics[1]:F2}");
                            writer.WriteLine($"CPU Utilization (%),{metrics[2]:F2}");
                            writer.WriteLine($"Throughput (processes/unit),{metrics[3]:F4}");
                            writer.WriteLine($"Average Response Time,{metrics[4]:F2}");
                            writer.WriteLine($"First Arrival Time,{metrics[5]}");
                            writer.WriteLine($"Last Finish Time,{metrics[6]}");
                            writer.WriteLine($"Total Elapsed Time,{metrics[7]}");
                            writer.WriteLine($"Total Burst Time,{metrics[8]}");
                        }
                        
                        MessageBox.Show($"Results exported successfully to:\n{saveDialog.FileName}", 
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}", 
                            "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes the process data table structure.
        /// </summary>
        private void InitializeProcessTable()
        {
            processTable = new DataTable();
            processTable.Columns.Add("Process ID", typeof(string));
            processTable.Columns.Add("Burst Time", typeof(int));
            processTable.Columns.Add("Priority", typeof(int));
            processTable.Columns.Add("Arrival Time", typeof(int));

            processDataGrid.DataSource = processTable;
            processDataGrid.AllowUserToAddRows = false;
            processDataGrid.AllowUserToDeleteRows = false;
            
            // Set column widths and configure for larger datasets
            if (processDataGrid.Columns.Count > 0)
            {
                processDataGrid.Columns[0].Width = 100; // Process ID
                processDataGrid.Columns[1].Width = 100; // Burst Time
                processDataGrid.Columns[2].Width = 100; // Priority  
                processDataGrid.Columns[3].Width = 100; // Arrival Time
                
                // STUDENTS: Performance optimizations for larger datasets
                processDataGrid.VirtualMode = false; // Set to true if using 500+ processes
                processDataGrid.RowHeadersVisible = false; // Save space
                processDataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None; // Faster rendering
            }
        }

        /// <summary>
        /// Handles the Set Process Count button click.
        /// </summary>
        private void SetProcessCount_Click(object sender, EventArgs e)
        {
            // STUDENTS: Process count validation using helper method
            // Adjust MIN/MAX_PROCESS_COUNT constants above for your requirements
            if (IsValidProcessCount(txtProcess.Text, out int processCount))
            {
                // STUDENTS: Performance warning for large datasets
                if (processCount > 50)
                {
                    var result = MessageBox.Show(
                        $"You are creating {processCount} processes. This may impact performance.\n\nContinue?",
                        "Large Dataset Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);
                    
                    if (result == DialogResult.No)
                    {
                        txtProcess.Focus();
                        return;
                    }
                }
                
                processTable.Clear();
                
                for (int i = 0; i < processCount; i++)
                {
                    DataRow row = processTable.NewRow();
                    row["Process ID"] = $"P{i + 1}";
                    row["Burst Time"] = random.Next(1, 11); // Default 1-10
                    row["Priority"] = i + 1; // Default priority
                    row["Arrival Time"] = 0; // Default arrival time
                    processTable.Rows.Add(row);
                }

                // Reset combo box selection
                cmbLoadExample.SelectedIndex = 0;
            }
            else
            {
                MessageBox.Show($"Please enter a valid number of processes ({MIN_PROCESS_COUNT}-{MAX_PROCESS_COUNT})", 
                    "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProcess.Focus();
            }
        }

        /// <summary>
        /// Generates random data for the process table.
        /// </summary>
        private void GenerateRandom_Click(object sender, EventArgs e)
        {
            foreach (DataRow row in processTable.Rows)
            {
                row["Burst Time"] = random.Next(1, 21);
                row["Priority"] = random.Next(1, processTable.Rows.Count + 1);
                row["Arrival Time"] = random.Next(0, 10);
            }
        }

        /// <summary>
        /// Clears all process data and resets to default state.
        /// </summary>
        private void ClearAll_Click(object sender, EventArgs e)
        {
            processTable.Clear();
            txtProcess.Text = DEFAULT_PROCESS_COUNT.ToString();
            cmbLoadExample.SelectedIndex = 0;
            txtProcess.Focus();
        }

        /// <summary>
        /// Loads example process scenarios.
        /// </summary>
        private void LoadExample_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbLoadExample.SelectedIndex <= 0 || processTable.Rows.Count == 0)
                return;

            switch (cmbLoadExample.SelectedIndex)
            {
                case 1: // Short Processes
                    foreach (DataRow row in processTable.Rows)
                    {
                        row["Burst Time"] = random.Next(1, 6);
                        row["Priority"] = random.Next(1, 5);
                        row["Arrival Time"] = 0;
                    }
                    break;
                    
                case 2: // Mixed Load
                    foreach (DataRow row in processTable.Rows)
                    {
                        row["Burst Time"] = random.Next(1, 21);
                        row["Priority"] = random.Next(1, 10);
                        row["Arrival Time"] = random.Next(0, 5);
                    }
                    break;
                    
                case 3: // Heavy Load
                    foreach (DataRow row in processTable.Rows)
                    {
                        row["Burst Time"] = random.Next(10, 31);
                        row["Priority"] = random.Next(1, 5);
                        row["Arrival Time"] = random.Next(0, 10);
                    }
                    break;
                    
                case 4: // Priority Demo
                    int priority = processTable.Rows.Count;
                    foreach (DataRow row in processTable.Rows)
                    {
                        row["Burst Time"] = random.Next(5, 15);
                        row["Priority"] = priority--;
                        row["Arrival Time"] = 0;
                    }
                    break;
            }
            
            cmbLoadExample.SelectedIndex = 0; // Reset dropdown
        }

        /// <summary>
        /// STUDENTS: Saves DataGrid data to CSV file for external editing or backup
        /// This allows you to prepare process data in Excel/CSV editors
        /// </summary>
        private void SaveData_Click(object sender, EventArgs e)
        {
            if (processTable.Rows.Count == 0)
            {
                MessageBox.Show("No process data to save. Please set process count first.", 
                    "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveDialog.DefaultExt = "csv";
                saveDialog.FileName = "ProcessData.csv";
                saveDialog.Title = "Save Process Data";

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var writer = new System.IO.StreamWriter(saveDialog.FileName))
                        {
                            // Write header
                            writer.WriteLine("Process ID,Burst Time,Priority,Arrival Time");
                            
                            // Write data rows
                            foreach (DataRow row in processTable.Rows)
                            {
                                writer.WriteLine($"{row["Process ID"]},{row["Burst Time"]},{row["Priority"]},{row["Arrival Time"]}");
                            }
                        }
                        
                        MessageBox.Show($"Process data saved successfully to:\n{saveDialog.FileName}", 
                            "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error saving file: {ex.Message}", 
                            "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// STUDENTS: Loads process data from CSV file for testing custom datasets
        /// This allows you to prepare complex test scenarios in Excel/CSV editors
        /// </summary>
        private void LoadData_Click(object sender, EventArgs e)
        {
            using (var openDialog = new OpenFileDialog())
            {
                openDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openDialog.DefaultExt = "csv";
                openDialog.Title = "Load Process Data from CSV";

                if (openDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var loadedData = new List<ProcessData>();
                        using (var reader = new System.IO.StreamReader(openDialog.FileName))
                        {
                            // Skip header line
                            var headerLine = reader.ReadLine();
                            if (headerLine == null)
                            {
                                MessageBox.Show("The CSV file is empty.", "Load Error", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            string line;
                            int lineNumber = 1;
                            while ((line = reader.ReadLine()) != null)
                            {
                                lineNumber++;
                                var parts = line.Split(',');
                                
                                if (parts.Length != 4)
                                {
                                    MessageBox.Show($"Invalid format on line {lineNumber}. Expected format: ProcessID,BurstTime,Priority,ArrivalTime", 
                                        "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }

                                try
                                {
                                    loadedData.Add(new ProcessData
                                    {
                                        ProcessID = parts[0].Trim(),
                                        BurstTime = int.Parse(parts[1].Trim()),
                                        Priority = int.Parse(parts[2].Trim()),
                                        ArrivalTime = int.Parse(parts[3].Trim())
                                    });
                                }
                                catch (FormatException)
                                {
                                    MessageBox.Show($"Invalid number format on line {lineNumber}.", 
                                        "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    return;
                                }
                            }
                        }

                        if (loadedData.Count == 0)
                        {
                            MessageBox.Show("No process data found in the CSV file.", "Load Error", 
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        if (loadedData.Count > MAX_PROCESS_COUNT)
                        {
                            MessageBox.Show($"CSV contains {loadedData.Count} processes, but maximum allowed is {MAX_PROCESS_COUNT}. Loading first {MAX_PROCESS_COUNT} processes.", 
                                "Process Count Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            loadedData = loadedData.Take(MAX_PROCESS_COUNT).ToList();
                        }

                        // Clear existing data and load from CSV
                        processTable.Clear();
                        foreach (var process in loadedData)
                        {
                            DataRow row = processTable.NewRow();
                            row["Process ID"] = process.ProcessID;
                            row["Burst Time"] = process.BurstTime;
                            row["Priority"] = process.Priority;
                            row["Arrival Time"] = process.ArrivalTime;
                            processTable.Rows.Add(row);
                        }

                        // Update UI to reflect loaded data
                        txtProcess.Text = loadedData.Count.ToString();
                        cmbLoadExample.SelectedIndex = 0;

                        MessageBox.Show($"Successfully loaded {loadedData.Count} processes from:\n{openDialog.FileName}", 
                            "Load Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading file: {ex.Message}", 
                            "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        /// <summary>
        /// Executes the First-Come, First-Served algorithm using DataGrid data.
        /// STUDENTS: This demonstrates how to use GetProcessDataFromGrid() instead of prompts
        /// Use this pattern for your custom algorithm implementations
        /// </summary>
        private void FirstComeFirstServeButton_Click(object sender, EventArgs e)
        {
            var processData = GetProcessDataFromGrid();
            if (processData.Count > 0)
            {
                // STUDENTS: Example implementation using DataGrid data
                var results = RunFCFSAlgorithm(processData);

                // Update Results tab with detailed scheduling results
                DisplaySchedulingResults(results, "FCFS - First Come First Serve");
                
                // Switch to Results panel and update sidebar
                ShowPanel(resultsPanel);
                sidePanel.Height = btnDashBoard.Height;
                sidePanel.Top = btnDashBoard.Top;
            }
            else
            {
                MessageBox.Show("Please set process count and ensure the data grid has process data.", 
                    "No Process Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProcess.Focus();
            }
        }

        /// <summary>
        /// Executes the Shortest Job First algorithm using DataGrid data.
        /// STUDENTS: Updated to use GetProcessDataFromGrid() instead of prompts
        /// Use this pattern for your custom algorithm implementations
        /// </summary>
        private void ShortestJobFirstButton_Click(object sender, EventArgs e)
        {
            var processData = GetProcessDataFromGrid();
            if (processData.Count > 0)
            {
                // STUDENTS: Updated implementation using DataGrid data
                var results = RunSJFAlgorithm(processData);

                // Update Results tab with detailed scheduling results
                DisplaySchedulingResults(results, "SJF - Shortest Job First");
                
                // Switch to Results panel and update sidebar
                ShowPanel(resultsPanel);
                sidePanel.Height = btnDashBoard.Height;
                sidePanel.Top = btnDashBoard.Top;
            }
            else
            {
                MessageBox.Show("Please set process count and ensure the data grid has process data.", 
                    "No Process Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProcess.Focus();
            }
        }

        /// <summary>
        /// Executes the Priority algorithm using DataGrid data.
        /// STUDENTS: Updated to use GetProcessDataFromGrid() instead of prompts
        /// Higher priority numbers = higher priority (1=lowest, higher numbers=higher priority)
        /// </summary>
        private void PriorityButton_Click(object sender, EventArgs e)
        {
            var processData = GetProcessDataFromGrid();
            if (processData.Count > 0)
            {
                // STUDENTS: Updated implementation using DataGrid data
                var results = RunPriorityAlgorithm(processData);

                // Update Results tab with detailed scheduling results
                DisplaySchedulingResults(results, "Priority Scheduling (Higher # = Higher Priority)");
                
                // Switch to Results panel and update sidebar
                ShowPanel(resultsPanel);
                sidePanel.Height = btnDashBoard.Height;
                sidePanel.Top = btnDashBoard.Top;
            }
            else
            {
                MessageBox.Show("Please set process count and ensure the data grid has process data.", 
                    "No Process Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProcess.Focus();
            }
        }

        //TODO: Button Click for the SRTF Algo, Maybe turn the event handler into a switch
        /// <summary>
        /// Executes the Shortest Remaining Time First Algo.
        /// This is just copied from the previous event handlers
        /// I only added the new algo.
        /// </summary>
        private void SRTFButton_Click(object sender, EventArgs e)
        {
            var processData = GetProcessDataFromGrid();

            if(processData.Count > 0)
            {
                var results = RunSRTFAlgorithm(processData);

                DisplaySchedulingResults(results, "SRTF - Shortest Remaining Time First");

                ShowPanel(resultsPanel);
                sidePanel.Height = btnDashBoard.Height;
                sidePanel.Top = btnDashBoard.Top;
            }
            else
            {
                MessageBox.Show("Please set process count and ensure the data grid has process data.",
                    "No Process Data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                txtProcess.Focus();
            }
        }

        //TODO: Button Click for the MLFQ Algo, Maybe turn the event handler into a switch
        /// <summary>
        /// Executes the Shortest Remaining Time First Algo.
        /// This is just copied from the previous event handlers
        /// I only added the new algo.
        /// </summary>
        private void MLFQButton_Click(object sender, EventArgs e)
        {
            var processData = GetProcessDataFromGrid();

            if(processData.Count > 0)
            {
                var results = RunMLFQAlgoritm(processData);

                DisplaySchedulingResults(results, "MLFQ - Multi-Level Feedback Queue");

                ShowPanel(resultsPanel);
                sidePanel.Height = btnDashBoard.Height;
                sidePanel.Top = btnDashBoard.Top;
            }
            else
            {
                MessageBox.Show("Please set process count and ensure the data grid has process data.",
                    "No Process Data",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                
                txtProcess.Focus();
            }
        }

        /// <summary>
        /// Occurs when the process count text changes.
        /// </summary>
        private void ProcessTextBox_TextChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Restarts the application.
        /// </summary>
        private void RestartApp_Click(object sender, EventArgs e)
        {
            Hide();
            CpuSchedulerForm cpuScheduler = new CpuSchedulerForm();
            cpuScheduler.ShowDialog();
        }



        /// <summary>
        /// STUDENTS: Applies rounded corners to a button for modern UI appearance
        /// Call this method for any custom buttons you add to maintain consistency
        /// </summary>
        private void ApplyRoundedCorners(Button button, int radius = 15)
        {
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, button.Width - 1, button.Height - 1);
            
            path.AddArc(rect.X, rect.Y, radius, radius, 180, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y, radius, radius, 270, 90);
            path.AddArc(rect.X + rect.Width - radius, rect.Y + rect.Height - radius, radius, radius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();
            
            button.Region = new Region(path);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
        }

        /// <summary>
        /// Handles form load logic.
        /// </summary>
        private void CpuSchedulerForm_Load(object sender, EventArgs e)
        {
            // Set default to Welcome panel
            sidePanel.Height = btnWelcome.Height;
            sidePanel.Top = btnWelcome.Top;
            listView1.View = View.Details;
            listView1.GridLines = true;
            
            // Initialize Results panel with placeholder message
            listView1.Clear();
            listView1.Columns.Add("Information", 400, HorizontalAlignment.Left);
            var welcomeItem = new ListViewItem("No results yet");
            welcomeItem.SubItems.Add("Run a scheduling algorithm to see results here");
            listView1.Items.Add(welcomeItem);
            
            // Initialize Welcome and About content
            InitializeWelcomeContent();
            InitializeAboutContent();
            
            // Load default process data for immediate use
            LoadDefaultProcessData();
            
            // Apply rounded corners to all buttons for modern UI
            ApplyRoundedCorners(btnSetProcessCount);
            ApplyRoundedCorners(btnGenerateRandom);
            ApplyRoundedCorners(btnClearAll);
            ApplyRoundedCorners(btnSaveData);
            ApplyRoundedCorners(btnLoadData);
            ApplyRoundedCorners(btnFCFS);
            ApplyRoundedCorners(btnSJF);
            ApplyRoundedCorners(btnPriority);
            ApplyRoundedCorners(btnRoundRobin);
            ApplyRoundedCorners(btnSRTF);
            ApplyRoundedCorners(btnMLFQ);
            ApplyRoundedCorners(btnDarkModeToggle);
            
            // Apply default dark theme
            ApplyTheme();
            
            // Show Welcome panel by default
            ShowPanel(welcomePanel);
        }

        /// <summary>
        /// STUDENTS: Loads default process data when the application starts
        /// This provides immediate usability without requiring manual setup
        /// </summary>
        private void LoadDefaultProcessData()
        {
            // Populate with 5 default processes for immediate testing
            for (int i = 0; i < 5; i++)
            {
                DataRow row = processTable.NewRow();
                row["Process ID"] = $"P{i + 1}";
                row["Burst Time"] = new int[] { 6, 8, 7, 3, 4 }[i]; // Interesting mix for learning
                row["Priority"] = i + 1; // Sequential priorities
                row["Arrival Time"] = new int[] { 0, 2, 4, 6, 8 }[i]; // Staggered arrivals
                processTable.Rows.Add(row);
            }

            // Set the process count text to match
            txtProcess.Text = "5";
            
            // Set combo box to default selection
            cmbLoadExample.SelectedIndex = 0;
        }

        /// <summary>
        /// STUDENTS: Applies dark or light theme to all UI elements
        /// This provides a modern interface that's easier on the eyes
        /// </summary>
        private void ApplyTheme()
        {
            if (isDarkMode)
            {
                ApplyDarkTheme();
                btnDarkModeToggle.Text = "☀️ Light Mode";
            }
            else
            {
                ApplyLightTheme();
                btnDarkModeToggle.Text = "🌙 Dark Mode";
            }
        }

        /// <summary>
        /// STUDENTS: Applies dark theme colors to all UI components
        /// </summary>
        private void ApplyDarkTheme()
        {
            // Main form background
            this.BackColor = Color.FromArgb(45, 45, 48);
            
            // Sidebar panel
            panel1.BackColor = Color.FromArgb(37, 37, 38);
            sidePanel.BackColor = Color.FromArgb(0, 122, 204); // Blue accent
            
            // All sidebar buttons
            ApplyDarkThemeToButton(btnWelcome);
            ApplyDarkThemeToButton(btnCpuScheduler);
            ApplyDarkThemeToButton(btnDashBoard);
            ApplyDarkThemeToButton(btnAbout);
            ApplyDarkThemeToButton(btnDarkModeToggle);
            
            // Restart label
            restartApp.BackColor = Color.FromArgb(37, 37, 38);
            restartApp.ForeColor = Color.FromArgb(241, 241, 241);
            
            // Copyright label
            label1.ForeColor = Color.FromArgb(153, 153, 153);
            
            // Content panels
            contentPanel.BackColor = Color.FromArgb(30, 30, 30);
            welcomePanel.BackColor = Color.FromArgb(30, 30, 30);
            schedulerPanel.BackColor = Color.FromArgb(30, 30, 30);
            resultsPanel.BackColor = Color.FromArgb(30, 30, 30);
            aboutPanel.BackColor = Color.FromArgb(30, 30, 30);
            
            // Text boxes
            welcomeTextBox.BackColor = Color.FromArgb(37, 37, 38);
            welcomeTextBox.ForeColor = Color.FromArgb(241, 241, 241);
            aboutTextBox.BackColor = Color.FromArgb(37, 37, 38);
            aboutTextBox.ForeColor = Color.FromArgb(241, 241, 241);
            
            // Process input controls
            labelProcess.ForeColor = Color.FromArgb(241, 241, 241);
            txtProcess.BackColor = Color.FromArgb(51, 51, 55);
            txtProcess.ForeColor = Color.FromArgb(241, 241, 241);
            
            // Data grid
            processDataGrid.BackgroundColor = Color.FromArgb(37, 37, 38);
            processDataGrid.DefaultCellStyle.BackColor = Color.FromArgb(51, 51, 55);
            processDataGrid.DefaultCellStyle.ForeColor = Color.FromArgb(241, 241, 241);
            processDataGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 45, 48);
            processDataGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(241, 241, 241);
            processDataGrid.GridColor = Color.FromArgb(62, 62, 66);
            
            // Combo box
            cmbLoadExample.BackColor = Color.FromArgb(51, 51, 55);
            cmbLoadExample.ForeColor = Color.FromArgb(241, 241, 241);
            
            // ListView (Results)
            listView1.BackColor = Color.FromArgb(37, 37, 38);
            listView1.ForeColor = Color.FromArgb(241, 241, 241);
            
            // All scheduler buttons with dark theme colors
            ApplyDarkThemeToSchedulerButton(btnSetProcessCount);
            ApplyDarkThemeToSchedulerButton(btnGenerateRandom);
            ApplyDarkThemeToSchedulerButton(btnClearAll);
            ApplyDarkThemeToSchedulerButton(btnSaveData);
            ApplyDarkThemeToSchedulerButton(btnLoadData);
            ApplyDarkThemeToSchedulerButton(btnFCFS);
            ApplyDarkThemeToSchedulerButton(btnSJF);
            ApplyDarkThemeToSchedulerButton(btnSRTF);
            ApplyDarkThemeToSchedulerButton(btnMLFQ);
            ApplyDarkThemeToSchedulerButton(btnPriority);
            ApplyDarkThemeToSchedulerButton(btnRoundRobin);
        }

        /// <summary>
        /// STUDENTS: Applies light theme colors to all UI components
        /// </summary>
        private void ApplyLightTheme()
        {
            // Main form background
            this.BackColor = SystemColors.Control;
            
            // Sidebar panel
            panel1.BackColor = SystemColors.InactiveBorder;
            sidePanel.BackColor = Color.SeaGreen;
            
            // All sidebar buttons
            ApplyLightThemeToButton(btnWelcome);
            ApplyLightThemeToButton(btnCpuScheduler);
            ApplyLightThemeToButton(btnDashBoard);
            ApplyLightThemeToButton(btnAbout);
            ApplyLightThemeToButton(btnDarkModeToggle);
            
            // Restart label
            restartApp.BackColor = SystemColors.InactiveBorder;
            restartApp.ForeColor = Color.DarkBlue;
            
            // Copyright label
            label1.ForeColor = SystemColors.ControlText;
            
            // Content panels
            contentPanel.BackColor = SystemColors.Control;
            welcomePanel.BackColor = SystemColors.Control;
            schedulerPanel.BackColor = SystemColors.Control;
            resultsPanel.BackColor = SystemColors.Control;
            aboutPanel.BackColor = SystemColors.Control;
            
            // Text boxes
            welcomeTextBox.BackColor = SystemColors.Window;
            welcomeTextBox.ForeColor = SystemColors.WindowText;
            aboutTextBox.BackColor = SystemColors.Window;
            aboutTextBox.ForeColor = SystemColors.WindowText;
            
            // Process input controls
            labelProcess.ForeColor = SystemColors.ControlText;
            txtProcess.BackColor = SystemColors.Window;
            txtProcess.ForeColor = SystemColors.WindowText;
            
            // Data grid
            processDataGrid.BackgroundColor = SystemColors.Window;
            processDataGrid.DefaultCellStyle.BackColor = SystemColors.Window;
            processDataGrid.DefaultCellStyle.ForeColor = SystemColors.WindowText;
            processDataGrid.ColumnHeadersDefaultCellStyle.BackColor = SystemColors.Control;
            processDataGrid.ColumnHeadersDefaultCellStyle.ForeColor = SystemColors.ControlText;
            processDataGrid.GridColor = SystemColors.ControlDark;
            
            // Combo box
            cmbLoadExample.BackColor = SystemColors.Window;
            cmbLoadExample.ForeColor = SystemColors.WindowText;
            
            // ListView (Results)
            listView1.BackColor = SystemColors.Window;
            listView1.ForeColor = SystemColors.WindowText;
            
            // All scheduler buttons with original light colors
            ApplyLightThemeToSchedulerButton(btnSetProcessCount);
            ApplyLightThemeToSchedulerButton(btnGenerateRandom);
            ApplyLightThemeToSchedulerButton(btnClearAll);
            ApplyLightThemeToSchedulerButton(btnSaveData);
            ApplyLightThemeToSchedulerButton(btnLoadData);
            
            // Algorithm buttons with their original colors
            btnFCFS.BackColor = Color.Beige;
            btnSJF.BackColor = Color.AntiqueWhite;
            btnPriority.BackColor = Color.Bisque;
            btnRoundRobin.BackColor = Color.PapayaWhip;
            
            // Reset text color for algorithm buttons
            btnFCFS.ForeColor = SystemColors.ControlText;
            btnSJF.ForeColor = SystemColors.ControlText;
            btnPriority.ForeColor = SystemColors.ControlText;
            btnRoundRobin.ForeColor = SystemColors.ControlText;
        }

        /// <summary>
        /// STUDENTS: Helper method to apply dark theme to sidebar buttons
        /// </summary>
        private void ApplyDarkThemeToButton(Button button)
        {
            button.BackColor = Color.FromArgb(37, 37, 38);
            button.ForeColor = Color.FromArgb(241, 241, 241);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(62, 62, 66);
        }

        /// <summary>
        /// STUDENTS: Helper method to apply light theme to sidebar buttons
        /// </summary>
        private void ApplyLightThemeToButton(Button button)
        {
            button.BackColor = SystemColors.InactiveBorder;
            button.ForeColor = SystemColors.ControlText;
            button.FlatAppearance.MouseOverBackColor = SystemColors.ButtonHighlight;
        }

        /// <summary>
        /// STUDENTS: Helper method to apply dark theme to scheduler buttons
        /// </summary>
        private void ApplyDarkThemeToSchedulerButton(Button button)
        {
            button.BackColor = Color.FromArgb(51, 51, 55);
            button.ForeColor = Color.FromArgb(241, 241, 241);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(0, 122, 204);
        }

        /// <summary>
        /// STUDENTS: Helper method to apply light theme to scheduler buttons
        /// </summary>
        private void ApplyLightThemeToSchedulerButton(Button button)
        {
            button.BackColor = SystemColors.ButtonFace;
            button.ForeColor = SystemColors.ControlText;
            button.FlatAppearance.MouseOverBackColor = Color.PaleGreen;
        }



        /// <summary>
        /// Executes the Round Robin algorithm using DataGrid data.
        /// STUDENTS: Updated to use GetProcessDataFromGrid() instead of prompts
        /// Each process gets a time quantum (default 4) before switching to next process
        /// </summary>
        private void RoundRobinButton_Click(object sender, EventArgs e)
        {
            var processData = GetProcessDataFromGrid();
            if (processData.Count > 0)
            {
                // Prompt for quantum time - this is algorithm-specific parameter
                string quantumInput = Microsoft.VisualBasic.Interaction.InputBox(
                    "Enter quantum time for Round Robin scheduling:", 
                    "Quantum Time", 
                    "4");
                
                if (int.TryParse(quantumInput, out int quantumTime) && quantumTime > 0)
                {
                    // STUDENTS: Updated implementation using DataGrid data
                    var results = RunRoundRobinAlgorithm(processData, quantumTime);

                    // Update Results tab with detailed scheduling results
                    DisplaySchedulingResults(results, $"Round Robin (Quantum = {quantumTime})");
                    
                    // Switch to Results panel and update sidebar
                    ShowPanel(resultsPanel);
                    sidePanel.Height = btnDashBoard.Height;
                    sidePanel.Top = btnDashBoard.Top;
                }
                else
                {
                    MessageBox.Show("Please enter a valid quantum time (positive integer).", 
                        "Invalid Quantum Time", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else
            {
                MessageBox.Show("Please set process count and ensure the data grid has process data.", 
                    "No Process Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtProcess.Focus();
            }
        }


    }

    /// <summary>
    /// STUDENTS: Custom button class with rounded edges for modern UI appearance
    /// You can use this for your custom algorithm buttons to maintain visual consistency
    /// </summary>
    public class RoundedButton : Button
    {
        private int borderRadius = 10;
        private Color borderColor = Color.FromArgb(200, 200, 200);

        public int BorderRadius
        {
            get { return borderRadius; }
            set { borderRadius = value; Invalidate(); }
        }

        public Color BorderColor
        {
            get { return borderColor; }
            set { borderColor = value; Invalidate(); }
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Create rounded rectangle path
            GraphicsPath path = new GraphicsPath();
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);
            path.AddArc(rect.X, rect.Y, borderRadius, borderRadius, 180, 90);
            path.AddArc(rect.X + rect.Width - borderRadius, rect.Y, borderRadius, borderRadius, 270, 90);
            path.AddArc(rect.X + rect.Width - borderRadius, rect.Y + rect.Height - borderRadius, borderRadius, borderRadius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - borderRadius, borderRadius, borderRadius, 90, 90);
            path.CloseAllFigures();

            // Set button region to rounded shape
            Region = new Region(path);

            // Fill background
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                g.FillPath(brush, path);
            }

            // Draw border
            using (Pen pen = new Pen(borderColor, 1))
            {
                g.DrawPath(pen, path);
            }

            // Draw text
            TextRenderer.DrawText(g, Text, Font, ClientRectangle, ForeColor, 
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

            path.Dispose();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }
    }
}