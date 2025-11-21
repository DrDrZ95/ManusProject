# Agent-UI Project Dependency Summary

This document summarizes all dependencies and development dependencies for the rewritten `agent-ui` project, as extracted from the `apps/agent-ui/package.json` file.

## Production Dependencies (`dependencies`)

These packages are required for the application to run in a production environment.

| Package Name | Version | Description |
| :--- | :--- | :--- |
| `lucide-react` | `^0.554.0` | A collection of beautiful, open-source icons. |
| `uuid` | `^13.0.0` | For generating universally unique identifiers. |
| `@google/genai` | `^1.30.0` | Google GenAI SDK for interacting with Gemini models. |
| `zustand` | `^5.0.8` | A small, fast, and scalable state-management solution. |
| `framer-motion` | `^12.23.24` | A production-ready motion library for React. |
| `clsx` | `^2.1.1` | A tiny utility for constructing className strings conditionally. |
| `react-syntax-highlighter` | `^16.1.0` | Syntax highlighting component for React. |
| `react-markdown` | `^10.1.0` | Renders markdown as React components. |
| `remark-gfm` | `^4.0.1` | Remark plugin to support GitHub Flavored Markdown (GFM). |
| `react` | `^19.2.0` | The core React library. |
| `react-dom` | `^19.2.0` | React package for working with the DOM. |
| `date-fns` | `^4.1.0` | Modern JavaScript date utility library. |
| `@xterm/addon-fit` | `^0.10.0` | Xterm.js addon for fitting the terminal to the container. |
| `xterm` | `^5.3.0` | A terminal emulator for the browser. |
| `@xterm/addon-webgl` | `^0.18.0` | Xterm.js addon for WebGL rendering. |

## Development Dependencies (`devDependencies`)

These packages are only required for development and build processes.

| Package Name | Version | Description |
| :--- | :--- | :--- |
| `@types/node` | `^22.14.0` | TypeScript type definitions for Node.js. |
| `@vitejs/plugin-react` | `^5.0.0` | Vite plugin for React projects. |
| `typescript` | `~5.8.2` | TypeScript language. |
| `vite` | `^6.2.0` | Next generation frontend tooling. |

