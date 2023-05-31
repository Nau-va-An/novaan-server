## A small guide to testing

### What should be considered as a unit?

- A unit is the smallest portion of code that can be isolated and tested independently.
- We need to focus on what "can be isolated and tested independently"
- For me, it is functions or methods that purely involve in processing data. CRUD functions are not included and hence will not be tested

### How to test?

- Test the common case of everything (that is a unit). This will tell you when that code breaks after you make some change
- Test the edge cases of a few unusually complex code that you think will probably have errors
- Whenever you find a bug, write a test case to cover it before fixing it
- Add edge-cases tests to less critical code whenever someone has time to kill

### Acceptance criterias

- As we only test what we've deemed as critical code, test coverage percent is not an excellent metric to display our progress...
