## TASK: Mandatory Completion Audit  
  
This is the FINAL step in the agent execution plan. You CANNOT skip this.  
The agent may only return control to the user AFTER this audit PASSES.  
  
## ORIGINAL USER OBJECTIVE  
"""  
{{original_user_objective}}  
"""  
  
## COMPLETED TASK SUMMARY (ALL TASKS)  
{{all_completed_tasks_with_results}}  
  
## AUDIT PROTOCOL — Complete ALL sections:  
  
### SECTION 1: Objective Coverage Matrix  
For each stated requirement in the user's original objective, verify it was addressed:  
  
| # | User Requirement | Addressed By Task(s) | Evidence | Status |  
|---|-----------------|---------------------|----------|--------|  
| 1 | [requirement]  | [task_ids]          | [brief]  | ✅/❌  |  
  
### SECTION 2: Deliverable Verification  
List every deliverable/artifact produced during execution:  
- [TYPE] [DESCRIPTION] [LOCATION/REFERENCE]  
  
### SECTION 3: Known Limitations & Gaps  
List anything that was NOT completed or has known quality issues:  
- [TASK_ID] [DESCRIPTION] [REASON]  
  
### SECTION 4: Audit Verdict  
  
IF all requirements in Section 1 are ✅:  
→ Output: {"audit_status": "PASSED", "confidence": 0.0-1.0, "summary": "..."}  
→ The agent loop is now ALLOWED to terminate.  
  
IF any requirement in Section 1 is ❌:  
→ Output: {"audit_status": "FAILED", "failed_requirements": [...], "recovery_plan": [...]}  
→ The agent loop MUST continue. Generate recovery tasks and inject them.  
→ NEVER report PASSED to the user if any requirement is unmet.  
  
## HONESTY RULE  
If you are uncertain whether a requirement is met, mark it ❌, NOT ✅.  
A false PASSED is worse than a delayed PASSED.  