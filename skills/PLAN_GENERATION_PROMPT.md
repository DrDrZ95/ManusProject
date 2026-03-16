## TASK: Generate a Multi-Phase Execution Plan  
  
You are in the PLANNING phase. The user has provided the following objective:  
"""  
{{user_objective}}  
"""  
  
Additional context (prior conversations, constraints, resources available):  
"""  
{{context}}  
"""  
  
## YOUR OUTPUT MUST BE IN TWO PARTS:  
  
### PART 1 — todo.md (Human-Readable Plan)  
Generate a markdown file strictly following this schema:  
  
---  
# [PROJECT TITLE]  
  
**Agent Session ID**: {{session_id}}    
**Objective**: [One-sentence restatement of the user's goal]    
**Created At**: {{timestamp}}    
**Status**: IN_PROGRESS    
**Phases**: [N]    
**Total Tasks**: [N]    
  
---  
  
## Phase 1: [Phase Name] — [Phase Goal in one sentence]  
**Rationale**: [Why this phase must come first]  
  
- [ ] [TASK_001] [TYPE:SEARCH] [PRIORITY:HIGH] [DEPENDS_ON:none]  
      Description: [What exactly will be done]  
      Expected Output: [What success looks like]  
      Estimated Tokens: [~N]  
  
- [ ] [TASK_002] [TYPE:CODE] [PRIORITY:HIGH] [DEPENDS_ON:TASK_001]  
      Description: ...  
      Expected Output: ...  
  
## Phase 2: [Phase Name] — [Phase Goal]  
**Rationale**: ...  
  
- [ ] [TASK_00N] [TYPE:VERIFY] [PRIORITY:CRITICAL] [DEPENDS_ON:TASK_00N-1]  
      Description: Run final verification that ALL objectives from the user request are met.  
      Expected Output: A checklist confirming each user requirement is addressed.  
  
---  
**COMPLETION CRITERIA**: All tasks marked [x]. Final AUDIT task [TASK_AUDIT] confirms   
all original objectives from the user request are 100% addressed.  
  
---  
  
### PART 2 — Structured JSON (Machine-Consumable)  
Immediately after the markdown, output a JSON block (```json ... ```) in this exact schema:  
  
{  
  "session_id": "{{session_id}}",  
  "title": "string",  
  "description": "string",  
  "phases": [  
    {  
      "phase_index": 0,  
      "phase_name": "string",  
      "rationale": "string"  
    }  
  ],  
  "steps": [  
    {  
      "index": 0,  
      "text": "[TYPE:SEARCH] Task description here",  
      "type": "SEARCH",  
      "priority": "HIGH",  
      "depends_on": [],  
      "expected_output": "string",  
      "phase_index": 0  
    }  
  ],  
  "completion_criteria": "string",  
  "injection_policy": {  
    "max_injections_per_step": 3,  
    "allowed_injection_reasons": ["PREREQUISITE_MISSING", "ERROR_RECOVERY", "NEW_INFORMATION"]  
  }  
}  
  
## PLANNING CONSTRAINTS  
- Minimum 2 phases, maximum 8 phases.  
- Each phase must have at least 1 task and at most 12 tasks.  
- The LAST task of the LAST phase MUST always be type [TYPE:AUDIT].  
- Task types allowed: SEARCH, CODE, ANALYZE, VERIFY, WRITE, API_CALL, AUDIT, RECOVERY.  
- DO NOT plan tasks that are vague — each task must have a deterministic "Expected Output".  
- DO NOT include tasks you don't have tools for. Only plan what you CAN execute.