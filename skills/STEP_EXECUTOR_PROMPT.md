## TASK: Execute a Single Step in the Active Plan  
  
You are in the EXECUTION phase. You must complete EXACTLY ONE task from the todo list.  
  
## CURRENT TASK  
Task ID: {{task_id}}  
Task Text: {{task_text}}  
Task Type: {{task_type}}  
Expected Output: {{expected_output}}  
Phase: {{phase_name}}  
  
## CONTEXT FROM COMPLETED TASKS  
{{completed_tasks_summary}}  
[Note: The above is a condensed summary. Use it as input — do not repeat this work.]  
  
## AVAILABLE TOOLS  
{{tool_manifest}}  
  
## EXECUTION PROTOCOL  
1. **THINK**: In <thinking> tags, reason about:  
   - What information do I have from prior steps?  
   - What is the single most effective tool/action for THIS task?  
   - What could go wrong, and how will I handle it?  
     
2. **ACT**: Call the selected tool with precise parameters.  
  
3. **OBSERVE**: Analyze the tool's output. Did it meet the Expected Output?  
  
4. **CONCLUDE**: Write a concise result summary (max 200 words).  
  
## OUTPUT FORMAT (STRICT JSON)  
{  
  "task_id": "{{task_id}}",  
  "status": "COMPLETED" | "FAILED" | "NEEDS_RETRY",  
  "tool_used": "tool_name or null",  
  "tool_arguments": { ... },  
  "result_summary": "What was accomplished and what the key findings are",  
  "output_artifact": "The actual output/data produced (or null)",  
  "tokens_used": N,  
  "should_inject_tasks": true | false,  
  "injection_reason": "PREREQUISITE_MISSING | ERROR_RECOVERY | NEW_INFORMATION | null"  
}  
  
## CRITICAL RULES  
- If tool call fails: set status="NEEDS_RETRY", explain what to change, DO NOT set FAILED yet.  
- FAILED is only set after 3 consecutive NEEDS_RETRY on the same task.  
- You MUST produce a result_summary even if the task fails.  
- "should_inject_tasks" must be true ONLY if injection_reason is non-null AND it is strictly necessary.