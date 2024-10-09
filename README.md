# SpecBridge

SpecBridge is a tool to synchronise gherkin specifications to Atlassian Confluence. The purpose of this tool is to be able to share `.feature` files with stakeholders. 

## Prerequisites
- An API token with access to a Atlassian Confluence Space
- A pre created page on that space

## How it works

This tool indexes `.feature` files in the given directories and creates a page for each `.feature`. All pages are created as children under the given PageId.

## Usage

```
  -f, --features    paths to feature files. default is './'
  --url             Required. atlassian base url, for example
                    'https://nwwz.atlassian.net/'
  --space           Required. atlassian space key, for example 'SpecBridge'
  --page            Required. id of the page under which to place the
                    specifications
  --user            Required. username to authenticate with
  --password        Required. password to authenticate with
  --help            Display help screen.
  --version         Display version information.
```

