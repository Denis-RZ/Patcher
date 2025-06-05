namespace UniversalCodePatcher.Workflow
{
    public enum WorkflowStep { ProjectSetup, RuleManagement, Preview, Apply, Results }

    public class WorkflowEngine
    {
        public WorkflowStep CurrentStep { get; private set; } = WorkflowStep.ProjectSetup;

        public void MoveToStep(WorkflowStep step)
        {
            CurrentStep = step;
            // Validate transition logic here
        }

        public bool CanMoveToStep(WorkflowStep step)
        {
            return true; // Add validation
        }
    }
}
