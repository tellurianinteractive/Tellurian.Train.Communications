# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET library for controlling model trains using Z21, LocoNet, and XpressNet protocols. The library provides protocol-agnostic interfaces that allow applications to control train operations, accessories, programming, and system monitoring.

## .NET Version
This project targets .NET 10.0. and the .NET 10 SDK. This is only defined globally in Directory.Build.props.

## Project Structure Summary
- agent_docs/: Documentation for Claude. File names reflects the content. Please, read these when you need additional knowledge on the topic.
- - **Tellurian.Communications.Channels**: UDP transport layer - see `Tellurian.Communications.Channels/CLAUDE.md`
- **Tellurian.Trains.Interfaces**: Protocol-agnostic interfaces and types - see `Tellurian.Trains.Interfaces/CLAUDE.md`
- **Tellurian.Trains.Protocols.XpressNet**: XpressNet protocol implementation - see `Tellurian.Trains.Protocols.XpressNet/CLAUDE.md`
- **Tellurian.Protocols.LocoNet**: LocoNet protocol implementation - see `Tellurian.Protocols.LocoNet/CLAUDE.md`
- **Tellurian.Trains.Adapters.Z21**: Z21 command station adapter - see `Tellurian.Trains.Adapters.Z21/CLAUDE.md`
- **Specifications/**: Detailed coding conventions and naming standards

Each project has its own `CLAUDE.md` file with project-specific implementation details.

## Absolute paths
Avoid using absolute paths when you searching in the project.
If you need absolute paths, use:
- C:\Utveckling\Github\Tellurian.Trains\Train.Control\