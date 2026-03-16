## TASK: Evaluate and Generate Dynamic Task Injections  
  
The agent has completed task {{completed_task_id}} and flagged that new tasks may be required.  
  
## COMPLETED TASK RESULT  
{{completed_task_result_summary}}  
  
## INJECTION REASON REPORTED  
{{injection_reason}}  
  
## CURRENT TODO LIST (REMAINING TASKS)  
{{remaining_tasks_list}}  
  
## INJECTION DECISION PROTOCOL  
  
### STEP 1: NECESSITY CHECK (answer all 3 before proceeding)  
Answer YES/NO to each:  
- Q1: Will the existing remaining tasks fail or produce incorrect results WITHOUT this injection?  
- Q2: Is this injection caused by NEW information (not predictable at planning time)?  
- Q3: Can existing tasks be MODIFIED (not replaced) to handle this instead of injecting?  
  
### STEP 2: INJECTION GATE  
- If Q1=NO → DO NOT inject. Output: {"inject": false, "reason": "not necessary"}  
- If Q3=YES → DO NOT inject. Output: {"inject": false, "reason": "modify existing task instead", "modification_suggestion": "..."}  
- If Q1=YES AND Q2=YES AND Q3=NO → PROCEED to Step 3.  
  
### STEP 3: GENERATE INJECTED TASKS  
Output injected tasks in this JSON format:  
  
{  
  "inject": true,  
  "inject_after_index": {{current_task_index}},  
  "injected_tasks": [  
    {  
      "text": "[TYPE:RECOVERY][INJECTED] Task description",  
      "type": "RECOVERY",  
      "priority": "HIGH",  
      "depends_on": ["{{completed_task_id}}"],  
      "expected_output": "Specific measurable output",  
      "injection_reason": "{{injection_reason}}"  
    }  
  ],  
  "impact_on_plan": "Brief explanation of how these tasks change the overall plan"  
}  
  
## INJECTION LIMITS  
- Maximum 3 tasks per injection event.  
- Maximum 5 total INJECTED tasks across the entire plan execution.  
- NEVER inject tasks of type AUDIT or tasks that duplicate existing pending tasks.  
- Injected tasks inherit the priority of the task that triggered the injection.