# Variables
SOLUTION_FILE = GoogleAIStudio.sln
TEST_RESULTS_DIR = TestResults
COVERAGE_REPORT_DIR = coverage-report
DOTNET = dotnet

.PHONY: test coverage clean

# Default target
all: clean test coverage

# Clean previous test results and coverage reports
clean:
    @echo Cleaning previous test results...
    @if exist $(TEST_RESULTS_DIR) rd /s /q $(TEST_RESULTS_DIR)
    @if exist $(COVERAGE_REPORT_DIR) rd /s /q $(COVERAGE_REPORT_DIR)

# Run all tests
test:
    @echo Running tests...
    @$(DOTNET) test $(SOLUTION_FILE) --collect:"XPlat Code Coverage"

# Generate coverage report
coverage:
    @echo Generating coverage report...
    @$(DOTNET) tool install -g dotnet-reportgenerator-globaltool --ignore-failed-sources || true
    @reportgenerator -reports:$(TEST_RESULTS_DIR)/**/coverage.cobertura.xml -targetdir:$(COVERAGE_REPORT_DIR) -reporttypes:Html
    @echo Coverage report generated in $(COVERAGE_REPORT_DIR)
    @start $(COVERAGE_REPORT_DIR)\index.html