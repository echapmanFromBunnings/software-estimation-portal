# Software Estimator User Guide

## Table of Contents
1. [Getting Started](#getting-started)
2. [Creating Your First Estimate](#creating-your-first-estimate)
3. [Understanding Team Assignment](#understanding-team-assignment)
4. [Working with Functional Line Items](#working-with-functional-line-items)
5. [Managing Non-Functional Items](#managing-non-functional-items)
6. [Resource Rates and Cost Calculation](#resource-rates-and-cost-calculation)
7. [Exporting Estimates](#exporting-estimates)
8. [Import and Backup](#import-and-backup)
9. [Configuration Management](#configuration-management)
10. [Tips and Best Practices](#tips-and-best-practices)

## Getting Started

The Software Estimator is a web-based platform for creating detailed software development estimates. It helps you calculate project costs by breaking down work into functional components and supporting activities.

### Accessing the Platform
1. Navigate to the application URL (typically `http://localhost:5000` for local development)
2. You'll see the home page with project statistics
3. Click "Estimates" in the navigation to view all projects

### Key Concepts
- **Estimates**: Individual projects with their associated costs and timelines
- **Functional Line Items**: Core development work broken into sprints
- **Non-Functional Items**: Supporting activities like testing, deployment, documentation
- **Teams**: Groups of resources with specific roles and employment types
- **Resource Rates**: Hourly or daily rates for different roles

## Creating Your First Estimate

1. **Navigate to Estimates**: Click "Estimates" in the top navigation
2. **Create New**: Click the "New Estimate" button
3. **Basic Information**:
   - **Name**: Give your project a descriptive name
   - **Client**: Enter the client organization name
   - **Sprint Length**: Set the length of your sprints in days (default: 10)
   - **Team Assignment**: Select or enter the team that will work on this project
   - **Contingency**: Set a percentage buffer for unforeseen work (0-100%)

4. **Save**: Click "Save Estimate" to create your project

## Understanding Team Assignment

The platform supports team-based estimation with employment type tracking:

### Team Configuration
Teams are defined in the system with:
- **Team Name**: Descriptive name for the team
- **Team Members**: Roles and their quantities
- **Employment Types**: Each role is classified as:
  - **FullTime**: Permanent employees
  - **Contractor**: External contractors

### Available Teams
The system comes pre-configured with several teams:
- **Alpha Squad**: Full-time development team with frontend, backend, and QA roles
- **Beta Squad**: Mixed team with both full-time and contractor resources
- **Gamma Squad**: Specialized team with database expertise
- **Delta Squad**: Balanced team for medium projects
- **Consulting Team**: All contractor-based team for external engagements

### Team Assignment Process
1. When creating or editing an estimate, enter the team name in the "Team" field
2. The system will validate team availability and capacity
3. Team information (including employment types) will be displayed in estimate breakdowns

## Working with Functional Line Items

Functional line items represent the core development work:

### Adding Functional Items
1. In the estimate editor, find the "Functional Line Items" section
2. Click "Add Line Item"
3. Fill in:
   - **Title**: Brief description of the feature or component
   - **Pattern**: Select from predefined common patterns
   - **Sprints**: Estimated development time in sprints
   - **Notes**: Additional details or assumptions

### Common Patterns
The system includes predefined patterns such as:
- **Simple CRUD Operations**: Basic create, read, update, delete functionality
- **Complex Business Logic**: Advanced processing and calculations
- **Integration Points**: Third-party system connections
- **User Interface Components**: Frontend development work

### Sprint Calculation
- Each line item specifies the number of sprints required
- Total cost is calculated as: `sprints × team cost per sprint`
- Sprint costs are based on the assigned team's composition and rates

## Managing Non-Functional Items

Non-functional items cover supporting activities:

### Types of Supporting Activities
- **Testing & QA**: Quality assurance activities
- **DevOps & Deployment**: Infrastructure and deployment setup
- **Documentation**: Technical and user documentation
- **Project Management**: Coordination and planning activities
- **Security & Compliance**: Security reviews and compliance checks

### Adding Supporting Activities
1. Scroll to the "Non-Functional Items" section
2. Click "Add Supporting Activity"
3. Select from the dropdown of available activities
4. The system automatically calculates suggested hours based on functional work percentage
5. Review and adjust allocations as needed

### Automatic Calculations
- Supporting activities have suggested percentages of functional work
- Hours are automatically distributed across team roles based on configured weights
- Costs are calculated using role-specific hourly rates

## Resource Rates and Cost Calculation

### Setting Up Rates
1. In the estimate editor, find the "Resource Rates" section
2. Add rates for each role in your team:
   - **Role**: Job title (e.g., Senior Developer, QA Engineer)
   - **Hourly Rate**: Cost per hour (recommended)
   - **Daily Rate**: Cost per day (will be divided by 8 for hourly calculations)

### Rate Mapping
- The system automatically maps supporting activity allocations to your defined rates
- If a role isn't found, it falls back to the average rate across all roles
- Unmapped roles will show warnings with options to create mappings

### Cost Calculation Flow
1. **Functional Costs**: `Sprints × Team Cost per Sprint`
2. **Supporting Activity Costs**: `Percentage of Functional × Role Allocations × Hourly Rates`
3. **Contingency**: `(Functional + Non-Functional) × Contingency Percentage`
4. **Total**: `Functional + Non-Functional + Contingency`

## Exporting Estimates

The platform supports multiple export formats:

### Export Options
1. **JSON Export**: Machine-readable format for integration
2. **CSV Export**: Spreadsheet-compatible format
3. **PDF Export**: Professional client presentation format

### PDF Features
- Executive summary with key metrics
- Detailed functional breakdown
- Non-functional activity details with employment type breakdown
- Team composition and rates
- Professional formatting with charts and tables

### Accessing Exports
1. From the Estimates list, click the "Actions" dropdown for any estimate
2. Select your preferred export format
3. The file will download automatically

## Import and Backup

### Backup Your Data
1. Use the "Export All" feature to create a complete backup
2. This creates a JSON file with all estimates
3. Store this file safely for disaster recovery

### Importing Estimates
1. Click "Import" on the Estimates page
2. Upload a previously exported JSON file
3. Review the import preview
4. Confirm to add the estimates to your system

### Cloning Estimates
- Use the "Clone" feature to create copies of existing estimates
- Useful for similar projects or creating templates
- All functional and non-functional items are copied

## Configuration Management

### Functional Patterns (`config/common_patterns.json`)
- Define reusable development patterns
- Include average sprint estimates
- Hot-reloaded when file changes

### Supporting Activities (`config/supporting_activities.json`)
- Configure available non-functional activities
- Set suggested percentages and role allocations
- Mark baseline activities for automatic inclusion

### Team Configuration (`config/teams.json`)
- Define available teams and their composition
- Specify roles, quantities, and employment types
- Configure team capabilities and specializations

### Making Configuration Changes
1. Edit the JSON files in the `config` directory
2. Changes are automatically detected and applied
3. No application restart required
4. New estimates will use updated configurations

## Tips and Best Practices

### Estimation Accuracy
- **Start with Patterns**: Use predefined patterns as baselines
- **Break Down Work**: Create multiple smaller line items rather than few large ones
- **Include Buffer**: Always use contingency percentage (15-25% typical)
- **Review Regularly**: Update estimates as requirements become clearer

### Team Management
- **Match Skills to Work**: Ensure assigned teams have required capabilities
- **Consider Employment Types**: Account for contractor vs full-time availability
- **Resource Planning**: Check team capacity against project timelines
- **Rate Reviews**: Keep hourly rates current with market conditions

### Project Organization
- **Consistent Naming**: Use clear, descriptive names for estimates
- **Client Tracking**: Always specify client for reporting and filtering
- **Version Control**: Use estimate versions for significant changes
- **Documentation**: Include detailed notes for assumptions and decisions

### Export Strategy
- **Regular Backups**: Export data regularly for safekeeping
- **Client Presentations**: Use PDF exports for professional client presentations
- **Data Analysis**: Use CSV exports for further analysis in spreadsheet tools
- **Integration**: Use JSON exports for integration with other systems

### Configuration Management
- **Version Control**: Keep configuration files in version control
- **Environment-Specific**: Maintain different configs for dev/prod environments
- **Team Collaboration**: Document configuration changes for team members
- **Testing**: Test configuration changes in development before production

## Troubleshooting

### Common Issues
- **Missing Rates**: Ensure all team roles have corresponding resource rates
- **Team Validation**: Check that assigned teams exist in the configuration
- **Export Failures**: Verify all required fields are completed
- **Calculation Errors**: Review rate mappings and percentage allocations

### Getting Help
- Check the technical README.md for developer information
- Review configuration files for available options
- Use the application logs for detailed error information
- Contact your system administrator for configuration changes

---

*This user guide covers the core functionality of the Software Estimator platform. For technical implementation details, see the README.md file.*