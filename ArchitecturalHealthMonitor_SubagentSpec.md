# Architectural Health Monitor - Subagent Specification (Updated for 2024 Blazor Standards)

## Overview

This subagent specification defines an intelligent architectural health monitoring system designed to track, analyze, and guide the evolution of the Project Management Application according to the SimplifiedArchitecturePlan.md. The subagent provides continuous monitoring, boundary violation detection, and evolution guidance based on concrete metrics and decision matrices.

**UPDATED FOR 2024**: This specification has been updated to align with Microsoft's official Blazor guidance and industry best practices, removing outdated restrictions that conflict with modern Blazor development patterns.

## 2024 Blazor Standards Update

### **ACCEPTABLE PATTERNS** (No Longer Violations)

The following patterns are now considered **ACCEPTABLE** and will **NOT** be flagged as architectural violations:

1. **✅ Direct Service Injection in UI Components**
   ```csharp
   @inject ITaskService TaskService
   @inject IProjectService ProjectService
   ```
   - This is Microsoft's recommended pattern for Blazor components
   - Standard dependency injection pattern for Blazor applications

2. **✅ Mixed Application Layer Imports**
   ```csharp
   @using TheNextLevel.Application.Tasks.Services
   @using TheNextLevel.Application.Tasks.DTOs
   ```
   - Components naturally need both services and DTOs
   - Industry standard practice for Blazor applications

3. **✅ Cross-Domain Coordination Services**
   ```csharp
   @inject ProjectTaskCoordinationService CoordinationService
   ```
   - Valid pattern for complex cross-domain operations
   - Represents proper separation of concerns

4. **✅ Direct Service Calls in Components**
   ```csharp
   await TaskService.CompleteTaskAsync(taskId);
   var task = await TaskService.GetTaskByIdAsync(taskId);
   ```
   - Microsoft's recommended approach for Blazor components
   - Eliminates unnecessary abstraction layers

### **MAINTAINED RESTRICTIONS** (Still Violations)

These architectural boundaries are still enforced:

1. **🚫 Domain Layer Isolation** - Domain layers cannot reference each other
2. **🚫 Layer Direction Enforcement** - Domain cannot reference Application/Infrastructure
3. **🚫 Missing Domain Implementations** - Proper domain modeling required

### **Updated Health Scoring Examples**

#### ✅ **Now Acceptable** (Score: 100/100)
```csharp
// TaskCard.razor
@using TheNextLevel.Application.Tasks.Services  // ✅ Acceptable
@using TheNextLevel.Application.Tasks.DTOs      // ✅ Acceptable  
@inject ITaskService TaskService                // ✅ Acceptable

private async Task OnStatusChange()
{
    await TaskService.CompleteTaskAsync(taskId);  // ✅ Acceptable
}
```

#### 🚫 **Still Violations** (Score: 0/100)
```csharp
// Domain/Tasks/Task.cs
using TheNextLevel.Application.Tasks.Services;  // 🚫 Layer violation
using TheNextLevel.Domain.Projects;            // 🚫 Cross-domain violation

public class Task 
{
    private ITaskService _service; // 🚫 Domain depending on Application
}
```

#### ⚠️ **Improved Scoring Logic**
- **Previous**: UI service injection was penalized (-10 points each)
- **Updated**: UI service injection is ignored (no penalty)
- **Result**: Projects following Microsoft Blazor guidance now score 90%+ instead of 50%

## Core Responsibilities

### 1. Domain Boundary Monitoring
- **Track cross-domain dependencies** in real-time
- **Detect namespace violations** between Projects, Tasks, and Users domains
- **Monitor domain coupling metrics** to prevent architectural drift
- **Enforce layered architecture** (Domain → Application → Infrastructure → Presentation)

### 2. Evolution Phase Management
- **Monitor Phase 1** (Single Project) health indicators
- **Detect transition triggers** for Phase 2 (Selective Extraction)
- **Guide Phase 3** (Full Separation) readiness assessment
- **Provide evolution recommendations** based on concrete metrics

### 3. Code Quality & Performance Tracking
- **Build time monitoring** to detect performance degradation
- **Test coverage analysis** per domain
- **Merge conflict tracking** as team collaboration indicator
- **Bundle size monitoring** for mobile performance

### 4. Architectural Violation Detection
- **Real-time boundary analysis** using static code analysis
- **Dependency direction validation** (prevent circular dependencies)
- **Naming convention enforcement** per domain
- **Infrastructure leak detection** into domain layers

## Subagent Capabilities

### Core Analysis Functions

```typescript
interface ArchitecturalHealthMonitor {
  // Primary monitoring functions
  analyzeDomainBoundaries(): Promise<DomainBoundaryReport>;
  checkEvolutionReadiness(): Promise<EvolutionRecommendation>;
  detectArchitecturalViolations(): Promise<ViolationReport>;
  assessPerformanceMetrics(): Promise<PerformanceReport>;
  
  // Continuous monitoring
  startContinuousMonitoring(): void;
  generateHealthDashboard(): Promise<HealthDashboard>;
  
  // Evolution guidance
  recommendNextSteps(): Promise<EvolutionGuidance>;
  generateMigrationPlan(targetPhase: ArchitecturePhase): Promise<MigrationPlan>;
}
```

### Domain Boundary Analysis

```typescript
interface DomainBoundaryReport {
  // Boundary health metrics
  crossDomainDependencyCount: number;
  domainCouplingScore: number;
  layerViolations: LayerViolation[];
  namespaceCompliance: NamespaceComplianceReport;
  
  // Detailed findings
  violations: BoundaryViolation[];
  recommendations: BoundaryRecommendation[];
  complianceScore: number; // 0-100
  
  // Trend data
  trendData: HealthTrend[];
}

interface BoundaryViolation {
  type: 'CrossDomainDependency' | 'LayerViolation' | 'NamespaceViolation';
  severity: 'Critical' | 'High' | 'Medium' | 'Low';
  sourceFile: string;
  targetFile: string;
  description: string;
  suggestedFix: string;
  ruleReference: string;
}
```

### Evolution Phase Management

```typescript
interface EvolutionRecommendation {
  currentPhase: ArchitecturePhase;
  recommendedPhase: ArchitecturePhase;
  confidence: number; // 0-100
  
  // Decision matrix evaluation
  teamSizeScore: number;
  deploymentFrequencyScore: number;
  domainCouplingScore: number;
  performanceScore: number;
  operationalComplexityScore: number;
  
  // Specific triggers
  triggeredCriteria: EvolutionTrigger[];
  blockers: EvolutionBlocker[];
  
  // Action plan
  nextSteps: EvolutionStep[];
  estimatedEffort: string;
  riskAssessment: RiskAssessment;
}

interface EvolutionTrigger {
  criterion: string;
  currentValue: number;
  threshold: number;
  impact: 'High' | 'Medium' | 'Low';
  description: string;
}
```

### Performance & Quality Monitoring

```typescript
interface PerformanceReport {
  // Build performance
  buildMetrics: {
    averageBuildTime: number; // milliseconds
    buildTimeHistory: TimeSeries[];
    buildSuccessRate: number;
  };
  
  // Test metrics
  testMetrics: {
    coverageByDomain: DomainCoverage[];
    testExecutionTime: number;
    testSuccessRate: number;
    testDistribution: TestDistribution;
  };
  
  // Team collaboration
  collaborationMetrics: {
    mergeConflictRate: number;
    averageCodeReviewTime: number;
    parallelDevelopmentConflicts: number;
  };
  
  // Application performance
  applicationMetrics: {
    startupTime: number;
    memoryUsage: number;
    bundleSize: number;
    responsivenessscore: number;
  };
}
```

## Monitoring Rules & Thresholds

### Phase 1 (Single Project) Health Indicators

```yaml
phase1_thresholds:
  domain_boundaries:
    # UPDATED: More lenient on cross-domain dependencies due to acceptable Blazor patterns
    max_cross_domain_dependencies: 15  # Increased from 10
    min_compliance_score: 90           # Increased due to clearer acceptable patterns
    max_layer_violations: 3            # Decreased - stricter on actual violations
  
  # NEW: Blazor-specific acceptable patterns
  blazor_patterns:
    ui_service_injection: "allowed"     # @inject IService in components
    mixed_namespace_imports: "allowed"  # Application.Services + Application.DTOs
    coordination_service_usage: "allowed" # Complex cross-domain operations
    direct_service_calls: "allowed"     # await Service.Method() in components
  
  performance:
    max_build_time_minutes: 2
    min_test_coverage_percent: 80
    max_startup_time_seconds: 3
  
  team_collaboration:
    max_merge_conflicts_per_week: 5
    max_code_review_hours: 8
  
  application_health:
    max_bundle_size_mb: 50
    min_responsiveness_score: 85
```

### Phase 2 Transition Triggers

```yaml
phase2_triggers:
  team_growth:
    min_developers: 4
    parallel_feature_teams: 2
  
  domain_complexity:
    cross_domain_violations: 15
    merge_conflict_rate: 20
    build_time_minutes: 5
  
  business_requirements:
    different_deployment_schedules: true
    domain_specific_scaling: true
    separate_team_ownership: true
```

### Phase 3 Readiness Indicators

```yaml
phase3_readiness:
  team_maturity:
    min_developers: 8
    microservice_experience: true
    devops_maturity: 'advanced'
  
  technical_requirements:
    independent_scaling: true
    different_tech_stacks: true
    complex_business_rules: true
  
  operational_capabilities:
    monitoring_infrastructure: true
    deployment_automation: true
    service_mesh_knowledge: true
```

## Implementation Strategy

### 1. Static Code Analysis Engine

```csharp
public class ArchitecturalAnalyzer
{
    private readonly ICodeAnalysisService _codeAnalysis;
    private readonly IMetricsCollector _metricsCollector;
    private readonly IThresholdManager _thresholds;
    
    public async Task<DomainBoundaryReport> AnalyzeDomainBoundariesAsync()
    {
        // Analyze namespace dependencies
        var dependencies = await _codeAnalysis.AnalyzeDependenciesAsync();
        
        // Check domain isolation
        var violations = await CheckDomainIsolationAsync(dependencies);
        
        // Validate layer architecture
        var layerViolations = await CheckLayerComplianceAsync(dependencies);
        
        // Calculate compliance scores
        var complianceScore = CalculateComplianceScore(violations, layerViolations);
        
        return new DomainBoundaryReport
        {
            CrossDomainDependencyCount = violations.Count(v => v.Type == ViolationType.CrossDomain),
            Violations = violations,
            LayerViolations = layerViolations,
            ComplianceScore = complianceScore,
            Recommendations = GenerateRecommendations(violations)
        };
    }
    
    private async Task<List<BoundaryViolation>> CheckDomainIsolationAsync(
        IEnumerable<CodeDependency> dependencies)
    {
        var violations = new List<BoundaryViolation>();
        
        foreach (var dependency in dependencies)
        {
            // Check Projects domain isolation
            if (IsInDomain(dependency.Source, "Projects") && 
                IsInDomain(dependency.Target, "Tasks"))
            {
                violations.Add(new BoundaryViolation
                {
                    Type = ViolationType.CrossDomain,
                    Severity = Severity.High,
                    SourceFile = dependency.Source,
                    TargetFile = dependency.Target,
                    Description = "Projects domain should not directly reference Tasks domain",
                    SuggestedFix = "Use domain events or coordination services for cross-domain communication",
                    RuleReference = "SimplifiedArchitecturePlan.md#domain-boundaries"
                });
            }
            
            // Check layer violations
            if (IsInLayer(dependency.Source, "Domain") && 
                IsInLayer(dependency.Target, "Application"))
            {
                violations.Add(new BoundaryViolation
                {
                    Type = ViolationType.LayerViolation,
                    Severity = Severity.Critical,
                    SourceFile = dependency.Source,
                    TargetFile = dependency.Target,
                    Description = "Domain layer cannot depend on Application layer",
                    SuggestedFix = "Move shared interfaces to Domain layer or create separate shared contracts",
                    RuleReference = "SimplifiedArchitecturePlan.md#layer-details"
                });
            }
        }
        
        return violations;
    }
}
```

### 2. Continuous Monitoring Service

```csharp
public class ContinuousArchitecturalMonitor : BackgroundService
{
    private readonly ArchitecturalAnalyzer _analyzer;
    private readonly IMetricsCollector _metrics;
    private readonly INotificationService _notifications;
    private readonly ILogger<ContinuousArchitecturalMonitor> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PerformHealthCheckAsync();
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during architectural health check");
            }
        }
    }
    
    private async Task PerformHealthCheckAsync()
    {
        // Analyze current state
        var boundaryReport = await _analyzer.AnalyzeDomainBoundariesAsync();
        var performanceReport = await _analyzer.AnalyzePerformanceAsync();
        var evolutionReport = await _analyzer.AssessEvolutionReadinessAsync();
        
        // Store metrics
        await _metrics.RecordBoundaryMetricsAsync(boundaryReport);
        await _metrics.RecordPerformanceMetricsAsync(performanceReport);
        await _metrics.RecordEvolutionMetricsAsync(evolutionReport);
        
        // Check for critical violations
        var criticalViolations = boundaryReport.Violations
            .Where(v => v.Severity == Severity.Critical)
            .ToList();
            
        if (criticalViolations.Any())
        {
            await _notifications.SendCriticalViolationAlertAsync(criticalViolations);
        }
        
        // Check evolution triggers
        if (evolutionReport.RecommendedPhase != evolutionReport.CurrentPhase)
        {
            await _notifications.SendEvolutionRecommendationAsync(evolutionReport);
        }
    }
}
```

### 3. Evolution Decision Engine

```csharp
public class EvolutionDecisionEngine
{
    private readonly IMetricsRepository _metricsRepository;
    private readonly IThresholdConfiguration _thresholds;
    
    public async Task<EvolutionRecommendation> AssessEvolutionReadinessAsync()
    {
        var currentMetrics = await _metricsRepository.GetLatestMetricsAsync();
        var currentPhase = DetermineCurrentPhase(currentMetrics);
        
        // Evaluate decision matrix criteria
        var teamSizeScore = EvaluateTeamSize(currentMetrics.TeamMetrics);
        var deploymentScore = EvaluateDeploymentFrequency(currentMetrics.DeploymentMetrics);
        var couplingScore = EvaluateDomainCoupling(currentMetrics.BoundaryMetrics);
        var performanceScore = EvaluatePerformanceNeeds(currentMetrics.PerformanceMetrics);
        var complexityScore = EvaluateOperationalComplexity(currentMetrics.OperationalMetrics);
        
        // Determine recommended phase
        var recommendedPhase = DetermineRecommendedPhase(
            teamSizeScore, deploymentScore, couplingScore, 
            performanceScore, complexityScore);
        
        // Identify specific triggers
        var triggers = IdentifyEvolutionTriggers(currentMetrics, currentPhase);
        var blockers = IdentifyEvolutionBlockers(currentMetrics, recommendedPhase);
        
        return new EvolutionRecommendation
        {
            CurrentPhase = currentPhase,
            RecommendedPhase = recommendedPhase,
            Confidence = CalculateConfidence(teamSizeScore, deploymentScore, couplingScore),
            TriggeredCriteria = triggers,
            Blockers = blockers,
            NextSteps = GenerateNextSteps(currentPhase, recommendedPhase, triggers, blockers)
        };
    }
    
    private ArchitecturePhase DetermineRecommendedPhase(
        int teamSize, int deploymentFreq, int coupling, int performance, int complexity)
    {
        // Phase 1: Single Project
        if (teamSize <= 3 && coupling >= 7 && complexity <= 3)
            return ArchitecturePhase.SingleProject;
        
        // Phase 3: Full Separation
        if (teamSize >= 8 && deploymentFreq >= 8 && coupling <= 3)
            return ArchitecturePhase.FullSeparation;
        
        // Phase 2: Selective Extraction
        return ArchitecturePhase.SelectiveExtraction;
    }
}
```

### 4. Health Dashboard Generator

```csharp
public class HealthDashboardGenerator
{
    public async Task<HealthDashboard> GenerateDashboardAsync()
    {
        var boundaryHealth = await AnalyzeBoundaryHealthAsync();
        var performanceHealth = await AnalyzePerformanceHealthAsync();
        var evolutionStatus = await AnalyzeEvolutionStatusAsync();
        var trendAnalysis = await AnalyzeTrendsAsync();
        
        return new HealthDashboard
        {
            OverallHealthScore = CalculateOverallHealth(boundaryHealth, performanceHealth),
            BoundaryHealth = boundaryHealth,
            PerformanceHealth = performanceHealth,
            EvolutionStatus = evolutionStatus,
            Trends = trendAnalysis,
            Alerts = await GenerateAlertsAsync(),
            Recommendations = await GenerateRecommendationsAsync(),
            LastUpdated = DateTime.UtcNow
        };
    }
    
    private HealthScore CalculateOverallHealth(
        BoundaryHealthScore boundary, PerformanceHealthScore performance)
    {
        var weighted = (boundary.Score * 0.4) + (performance.Score * 0.6);
        
        return new HealthScore
        {
            Score = (int)weighted,
            Status = weighted >= 80 ? HealthStatus.Excellent :
                    weighted >= 60 ? HealthStatus.Good :
                    weighted >= 40 ? HealthStatus.Warning :
                    HealthStatus.Critical,
            Description = GenerateHealthDescription(weighted)
        };
    }
}
```

## Integration Points

### 1. Development Workflow Integration

```yaml
# CI/CD Pipeline Integration
pre_commit_hooks:
  - architectural_boundary_check
  - layer_violation_detection
  - namespace_compliance_check

build_pipeline:
  - run_architectural_tests
  - collect_boundary_metrics
  - generate_health_report
  - fail_on_critical_violations

deployment_pipeline:
  - validate_evolution_readiness
  - check_performance_regression
  - update_health_dashboard
```

### 2. IDE Integration

```csharp
// Visual Studio / VS Code Extension
public class ArchitecturalHealthExtension
{
    // Real-time boundary violation highlighting
    public void HighlightBoundaryViolations(Document document);
    
    // Code completion with architectural guidance
    public void ProvideArchitecturalIntelliSense();
    
    // Quick fixes for common violations
    public void SuggestArchitecturalFixes(Diagnostic diagnostic);
    
    // Health status in status bar
    public void UpdateHealthStatusIndicator();
}
```

### 3. Team Communication Integration

```csharp
// Slack/Teams Integration
public class TeamNotificationService
{
    public async Task SendDailyHealthReportAsync();
    public async Task AlertOnCriticalViolationsAsync(List<BoundaryViolation> violations);
    public async Task NotifyEvolutionRecommendationAsync(EvolutionRecommendation recommendation);
    public async Task ShareHealthDashboardAsync(HealthDashboard dashboard);
}
```

## Configuration & Customization

### 1. Threshold Configuration

```json
{
  "architecturalHealth": {
    "domainBoundaries": {
      "maxCrossDomainDependencies": 10,
      "minComplianceScore": 85,
      "criticalViolationThreshold": 5
    },
    "performance": {
      "maxBuildTimeMinutes": 2,
      "minTestCoveragePercent": 80,
      "maxBundleSizeMB": 50
    },
    "evolution": {
      "phase1Thresholds": {
        "maxTeamSize": 3,
        "maxBuildTime": 120,
        "maxMergeConflicts": 5
      },
      "phase2Triggers": {
        "minTeamSize": 4,
        "maxCrossDomainViolations": 15,
        "separateDeploymentNeeds": true
      },
      "phase3Readiness": {
        "minTeamSize": 8,
        "microserviceExperience": true,
        "independentScaling": true
      }
    }
  }
}
```

### 2. Custom Rules Engine

```csharp
public interface IArchitecturalRule
{
    string Name { get; }
    string Description { get; }
    Severity DefaultSeverity { get; }
    
    Task<RuleViolation[]> EvaluateAsync(CodeAnalysisContext context);
}

// Updated rule that respects 2024 Blazor patterns
public class ModernDomainBoundaryRule : IArchitecturalRule
{
    public string Name => "Domain Boundary Isolation (Blazor-Aware)";
    public string Description => "Domain layers must not directly reference each other, but UI components may use application services";
    public Severity DefaultSeverity => Severity.High;
    
    public async Task<RuleViolation[]> EvaluateAsync(CodeAnalysisContext context)
    {
        var violations = new List<RuleViolation>();
        
        foreach (var dependency in context.Dependencies)
        {
            // SKIP: Blazor component to application service (acceptable pattern)
            if (IsBlazorComponentToApplicationService(dependency))
                continue;
            
            // SKIP: UI component mixed imports (acceptable pattern)  
            if (IsUIComponentMixedImport(dependency))
                continue;
            
            // SKIP: Coordination service usage (acceptable pattern)
            if (IsCoordinationServiceUsage(dependency))
                continue;
            
            // ENFORCE: Domain-to-domain dependencies (still violations)
            if (IsDomainToDomainDependency(dependency))
            {
                violations.Add(new RuleViolation
                {
                    Rule = this,
                    Location = dependency.Location,
                    Message = "Direct domain-to-domain dependency detected",
                    SuggestedFix = "Use coordination services or domain events for cross-domain communication"
                });
            }
            
            // ENFORCE: Layer violations (still critical)
            if (IsDomainToApplicationLayerViolation(dependency))
            {
                violations.Add(new RuleViolation
                {
                    Rule = this,
                    Location = dependency.Location,
                    Message = "Domain layer cannot reference Application layer",
                    SuggestedFix = "Move shared interfaces to Domain layer or use dependency inversion"
                });
            }
        }
        
        return violations.ToArray();
    }
    
    private bool IsBlazorComponentToApplicationService(CodeDependency dependency) =>
        dependency.Source.Contains("Components") && dependency.Source.EndsWith(".razor") &&
        dependency.Target.Contains("Application") && dependency.Target.Contains("Services");
        
    private bool IsUIComponentMixedImport(CodeDependency dependency) =>
        dependency.Source.Contains("Components") && 
        (dependency.Target.Contains("Application.Tasks.Services") || 
         dependency.Target.Contains("Application.Tasks.DTOs"));
         
    private bool IsCoordinationServiceUsage(CodeDependency dependency) =>
        dependency.Target.Contains("Coordination") && dependency.Target.Contains("Service");
}
```

## Reporting & Analytics

### 1. Health Reports

```typescript
interface HealthReport {
  // Executive summary
  summary: {
    overallHealth: HealthStatus;
    criticalIssues: number;
    trendsDirection: 'Improving' | 'Stable' | 'Declining';
    recommendedActions: string[];
  };
  
  // Detailed sections
  domainBoundaryHealth: DomainBoundaryReport;
  performanceHealth: PerformanceReport;
  evolutionAssessment: EvolutionRecommendation;
  
  // Historical data
  trends: {
    healthScoreHistory: TimeSeries[];
    violationTrends: TimeSeries[];
    performanceTrends: TimeSeries[];
  };
  
  // Actionable insights
  recommendations: ActionableRecommendation[];
  prioritizedActions: PriorityAction[];
}
```

### 2. Evolution Roadmap

```typescript
interface EvolutionRoadmap {
  currentState: ArchitecturalState;
  targetState: ArchitecturalState;
  
  phases: EvolutionPhase[];
  
  timeline: {
    estimatedDuration: string;
    milestones: Milestone[];
    riskFactors: RiskFactor[];
  };
  
  resources: {
    requiredSkills: string[];
    estimatedEffort: string;
    teamImpact: string;
  };
}
```

## Success Metrics

### 1. Health Monitoring Effectiveness

- **Violation Detection Rate**: Percentage of architectural violations caught automatically
- **False Positive Rate**: Accuracy of violation detection (target: <5%)
- **Time to Detection**: Average time between violation introduction and detection
- **Resolution Time**: Average time from detection to violation resolution

### 2. Evolution Guidance Accuracy

- **Recommendation Accuracy**: Percentage of evolution recommendations followed successfully
- **Migration Success Rate**: Success rate of guided architectural migrations
- **Team Productivity Impact**: Development velocity before/after recommendations
- **Quality Improvement**: Reduction in bugs and technical debt

### 3. Team Adoption Metrics

- **Daily Active Usage**: Percentage of team members using health monitoring daily
- **Rule Compliance Rate**: Percentage increase in architectural rule compliance
- **Knowledge Transfer**: Reduction in architectural onboarding time
- **Decision Support**: Percentage of architectural decisions backed by metrics

## Implementation Roadmap

### Phase 1: Foundation (Weeks 1-4)
1. Implement basic static code analysis for domain boundaries
2. Create violation detection rules for the current project structure
3. Set up basic metrics collection and storage
4. Implement simple health scoring algorithm

### Phase 2: Monitoring (Weeks 5-8)
1. Add continuous monitoring background service
2. Implement performance and build time tracking
3. Create basic health dashboard
4. Add alerting for critical violations

### Phase 3: Evolution Guidance (Weeks 9-12)
1. Implement evolution decision engine
2. Add migration planning capabilities
3. Create detailed reporting and analytics
4. Integrate with development workflow

### Phase 4: Advanced Features (Weeks 13-16)
1. Add custom rules engine
2. Implement trend analysis and predictions
3. Create IDE integrations
4. Add team collaboration features

This architectural health monitoring subagent provides comprehensive guidance for maintaining and evolving the Project Management Application according to the established architectural plan, ensuring that complexity is managed effectively while preserving the ability to grow and adapt as requirements change.