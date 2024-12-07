using System.Text;
using Gherkin;
using Gherkin.Ast;

namespace App;

public class GherkinParser
{
    private readonly Parser _parser;

    public GherkinParser()
    {
        _parser = new Parser();
    }

    public ConfluenceDocument ParseToConfluence(TextReader reader)
    {
        var specifications = _parser.Parse(reader);

        return ToConfluence(specifications);
    }

    private ConfluenceDocument ToConfluence(GherkinDocument specifications)
    {
        var contentbuilder = new StringBuilder();
        if (specifications.Feature.Description != null)
        {
            var description = specifications.Feature.Description.Split("\n").Select(x => x.Trim());

            foreach (var line in description)
            {
                contentbuilder.Append($"<p>{line}</p>");
            }
        }

        var publish = specifications.Feature.Tags.All(t => t.Name != "@no_publish");

        foreach (var child in specifications.Feature.Children)
        {
            switch (child)
            {
                case Scenario scenario:
                    RenderScenario(scenario, contentbuilder);
                    break;
                case Rule rule:
                    RenderRule(rule, contentbuilder);
                    break;
            }
        }
        
        return new ConfluenceDocument(specifications.Feature.Name, contentbuilder.ToString(), publish);
    }

    private void RenderRule(Rule rule, StringBuilder builder)
    {
        var ruleText = $"Rule: {rule.Name}";
        builder.Append(
            $"<ac:structured-macro ac:name=\"tip\" ac:schema-version=\"1\" ac:macro-id=\"3c9c0069-88dd-4a2b-8fee-076564d6faa8\"><ac:rich-text-body><p>{ruleText}</p></ac:rich-text-body></ac:structured-macro>");
        foreach (var child in rule.Children)
        {
            switch (child)
            {
                case Scenario scenario:
                    RenderScenario(scenario, builder);
                    break;
            }
        }
    }

    private void RenderScenario(Scenario scenario, StringBuilder builder)
    {
        builder.Append($"<h3>{scenario.Name}</h3>");

        foreach (var step in scenario.Steps)
        {
            builder.Append("<blockquote>");
            builder.Append(
                $"<p><strong><span style=\"color: rgb(101,84,192);\">{step.Keyword}</span></strong> {step.Text.Replace("<", "&lt;").Replace(">", "&gt;")}</p>");
            builder.Append("</blockquote>");
            
            if (step.Argument is DataTable dt)
            {
                var firstRow = true;
                builder.Append(
                    "<table data-table-width=\"760\" data-layout=\"default\" ac:local-id=\"2c81202d-47d6-4449-be3b-cc6b93b87b27\"><tbody>");

                foreach (var row in dt.Rows)
                {
                    builder.Append("<tr>");
                    if (firstRow)
                    {
                        foreach (var cell in row.Cells)
                        {
                            builder.Append($"<th><h6>{cell.Value}</h6></th>");
                        }
                        firstRow = false;
                    }
                    else
                    {
                        foreach (var cell in row.Cells)
                        {
                            builder.Append($"<td><h6>{cell.Value}</h6></td>");
                        }
                    }
                    builder.Append("</tr>");
                }
                builder.Append("</tbody></table>");
            }
        }

        if (scenario.Examples.Any())
        {
            foreach (var example in scenario.Examples)
            {
                builder.Append(
                    "<table data-table-width=\"760\" data-layout=\"default\" ac:local-id=\"2c81202d-47d6-4449-be3b-cc6b93b87b27\"><tbody>");
                builder.Append("<tr>");
                foreach (var cell in example.TableHeader.Cells)
                {
                    builder.Append($"<th><p><strong>{cell.Value}</strong></p></th>");
                }

                builder.Append("</tr>");
                
                foreach (var row in example.TableBody)
                {
                    builder.Append("<tr>");
                    foreach (var cell in row.Cells)
                    {
                        builder.Append($"<td>{cell.Value}</td>");
                    }
                    
                    builder.Append("</tr>");
                }

                builder.Append("</tbody></table>");
            }
        }
    }
}

public record ConfluenceDocument(string Title, string Content, bool Publish);