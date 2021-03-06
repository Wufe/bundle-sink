# BundleSink

![.NET Core](https://github.com/Wufe/bundle-sink/workflows/.NET%20Core/badge.svg)

Manage static assets within an ASP.NET Core application complying with common encapsulation practices.

***

# Table of contents

* [Getting started](#getting-started)
    * [Requirements](#requirements)
    * [Installation](#installation)
    * [Usage](#usage)
        * [The result](#the-result)
        * [Pay attention to execution order](#pay-attention-to-execution-order)
        * [Rewrite output mode](#rewrite-output-mode)
* [Additional options](#additional-options)
    * [Literal entries](#literal-entries)
    * [Partial builds](#partial-builds)


***

# Getting started


## Requirements

- The library currently supports `netcoreapp3.1` applications.
- You need a webpack configuration for your bundles.


## Installation

- From the dotnet CLI:  
`dotnet add package BundleSink`

- Import tag helpers within `_ViewImports.cshtml`:  
`@addTagHelper *, BundleSink`

- From a command line:
`npm install bundle-sink-webpack-plugin`

- Update your webpack.config.js adding the plugin:
```js
const BundleSinkWebpackPlugin = require('bundle-sink-webpack-plugin');

module.exports = env => {

    const bundleSinkWebpackPlugin = new BundleSinkWebpackPlugin({
        output: path.resolve(__dirname, 'client-manifest.json')
    });

    return {
        ...,
        plugins: [
            ...bundleSinkWebpackPlugin.plugins
        ]
    };
}
    
```

- Initialize BundleSink in Program.cs:

```csharp
public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder
                .ConfigureBundleSink(builder => {
                    builder.WithWebpack("client-manifest.json", "/dist/");
                })
                .UseStartup<Startup>();
        });
```

> The function `WithWebpack` requires two arguments:
> - The name of the assets manifest created with webpack
> - The **public** output folder (i.e. the folder under wwwroot where the output is being generated by webpack)


## Usage

- Place a sink in your `_Layout.cshtml` or a razor page's "Scripts" section:  

*e.g.:  

```razor
<h1>Homepage</h1>

@section Scripts {
    <sink />
}
```

- Use a webpack entry from within a partial view / view-component, defining **the name of the entry**

```razor
This is a partial view from the homepage

<webpack-entry name="my-homepage-feature" />
```

## The result
The `<webpack-entry>` tag helper marked the specified entry as a dependency.  
The `<sink>` tag helper uses the dependencies declared all over the razor pages, taking care of duplicates and required chunks

The html will contain the scripts required for the selected entries, with their hash appended as query string.  
*e.g.*
```html
<script type="text/javascript" src="/dist/my-homepage-feature.js?v=NMaMA8xzap806fSOec7CFpI78hl033lAOIq_Lrr4kmY"></script>
```

## Pay attention to execution order

The `<sink />` tag helper must be declared after all the `entries` gets declared, execution order wise.  

The `_Layout.cshtml` page can contain a `<sink />` tag helper as first istruction and a page imported with the `@RenderBody()` function can contain a `<webpack-entry />`, because the `@RenderBody` function delays the execution of the entire layout page:  
in other words, **the `<webpack-entry />` gets called first**.

At the same time, having the `entry` and the `sink` in the same page source **can prevent the entry to get printed**:  

**Wrong usage**:
```cshtml
<sink />
<webpack-entry name="homepage">
```

**Right usage**:
```cshtml
<webpack-entry name="homepage">
<sink />
```

If you really need to declare sinks and entries in different ways, you can try enabling the [Rewrite output mode](#rewrite-output-mode).

***

## Rewrite output mode

There's a mode you can activate that allows to declare entries and sinks in the order you prefer.  
This mode is called `RewriteOutput` and can be enabled while configuring bundle sink:

```csharp
webBuilder
    .ConfigureBundleSink(builder =>
    {
        builder.RewriteOutput = true;
        builder.WithWebpack("wwwroot/dist/client-manifest.json", "/dist/");
    });
```

This modality is a little **hacky**: mocks the HttpContext.Response.Body to allow it to be read from a custom ResultFilter.  
The performance implication of this operation has not been measured.  

**Use it at your own risk.**

***

# Additional options

The webpack-entry accepts more options in form of attributes
- Use the attribute `key` if you need an entry to be imported more than once ... Why would you?  (Their dependencies will be imported once)
- Use the attribute `async` to mark the entry (but not its dependencies) as async
- Use the attribute `defer` to mark the entry (but not its dependencies) as deferred
- Use the attribute `css-only` to use css assets only
- Use the attribute `js-only` to use js assets only

**Named sinks**  
You can also render a webpack-entry to a specific sink:
- Use the `name` attribute on the sink tag helper
- Use the `sink` attribute on the webpack-entry tag helper

*e.g.*
```razor
<webpack-entry sink="ABOVE" />
<sink name="ABOVE />
```

**Dependencies**  
You can mark a `<webpack-entry>` with the `requires` attribute containing a comma separated list of entries required.  
These dependencies will be printed before the dependant entry.  

At the same time you can mark an entry with the `required-by` attribute, which tells the library to prevent printing the entry if no dependants are declared.  

## Literal entries

You may want to use this library as a native ASPNET Core section, but with added functionalities:  
you can use the `<literal-entry>` tag helper.  

Its usage is the same as native section or environment tag helper.  

This helper supports `name`, `key`, `sink`, `requires` and `required-by` attributes.

## Partial builds

You may want to build one entry at a time, in order to speed up development processes.

The `bundle-sink-webpack-plugin` package provides an option called `partial` which needs to be set to `true` to merge the resulting `client-manifest.json` with the previous one.  

With `partial: true` the clean plugin gets disabled.  

**Automatic configuration**

The automatic configuration is provided by the plugin itself.  
Parses the `env` variable passed to the webpack config and checks whether it refers to an existing entry.

That's how you use it:  
```js
const BundleSinkWebpackPlugin = require('bundle-sink-webpack-plugin');
module.exports = env => {

    const bundleSinkWebpackPlugin = new BundleSinkWebpackPlugin({
        clean: true,
        output: path.resolve(__dirname, 'wwwroot/dist/client-manifest.json'),
        entry: {
            'page-a': './page-a/index.ts',
            'page-b': './page-b/index.ts'
        },
        env
    });

    return {
        entry: bundleSinkWebpackPlugin.entry,
        ...ADDITIONAL WEBPACK PARAMETERS,
        plugins: [
            ...bundleSinkWebpackPlugin.plugins
        ]
    };
};
```

You can call webpack this way:  
`webpack --config webpack.config.js --env only=page-a`

**Manual configuration**

You can always **manually** provide the correct `entry` object to webpack and update the bundle-sink options accordingly:  
(*e.g.*)
```js
const BundleSinkWebpackPlugin = require('bundle-sink-webpack-plugin');
module.exports = env => {

    let entry = {
        'page-a': './page-a/index.ts',
        'page-b': './page-b/index.ts'
    }

    const bundleSinkOptions = {
        clean: true,
        output: path.resolve(__dirname, 'wwwroot/dist/client-manifest.json'),
        partial: false
    };
    
    if (env['only']) {
        const selectedEntry = env['only'];
        if (!entry[selectedEntry]) {
            throw new Error(`Syntax: --env only=<entry name>`);
        }
        entry = { [selectedEntry]: entry[selectedEntry] };
        bundleSinkOptions.partial = true;
    }

    const bundleSinkWebpackPlugin = new BundleSinkWebpackPlugin(bundleSinkOptions);

    return {
        entry: entry,
        ...ADDITIONAL WEBPACK PARAMETERS,
        plugins: [
            ...bundleSinkWebpackPlugin.plugins
        ]
    };
};
```

***