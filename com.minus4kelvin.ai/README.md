# AI and Character Task System

### Dependencies
- https://github.com/khncao/com.minus4kelvin.core 
- Tested working on Unity 2020.3.6f1+

### Summary
- Simple finite state machine with some common implementations

### Key Types
- StateMachine: state pipe
- StateMachineManager: optional for consolidating StateMachine updates
- States: implements IState; from super states such as Task and Idle to micro commands such as WaitCommand and PathCommand
- Commands: IState with simple single actions or do not have nested states
- Wrappers: for editor editable state instantiating; can create own property drawer or use optional automatic SerializedReference package
- Interactables: managed scene components for processor scene interaction
- StateProcessor: AI agent; manages state machine

### Implementation Types(may move to examples)
- TaskManager: registry and management of global tasks; assignment to StateProcessors
- Certain states, commands, and interactables that are less general-use

### Todo
- Isolate to be standalone with no core dependency
- Additional states: processor-character interactions
- Interactable processor arranger for formations, queues, etc.
- Scene prewarm with active processors in various states and positions

- Cleaner state interruptions; recyclability
- Reduce allocations
  - structs not boxed when accessing interfaces through generic constraints
- Tests for main class functionality