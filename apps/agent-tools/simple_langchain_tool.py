#!/usr/bin/env python3

"""
This module provides a simple example of a LangChain tool.
It demonstrates how to define a basic function that can be used as a tool
within a LangChain agent or chain.

这个模块提供了一个简单的 LangChain 工具示例。
它演示了如何定义一个基本函数，该函数可以用作 LangChain 代理或链中的工具。
"""

from langchain.tools import tool

@tool
def get_current_weather(location: str) -> str:
    """
    Returns the current weather for a given location.
    This is a dummy function for demonstration purposes.

    返回给定位置的当前天气。
    这是一个用于演示目的的模拟函数。
    """
    if "san francisco" in location.lower():
        return "It's 72 degrees Fahrenheit and sunny in San Francisco."
    elif "new york" in location.lower():
        return "It's 68 degrees Fahrenheit and cloudy in New York."
    else:
        return "Weather information not available for this location."

# Example of how to use the tool (for local testing)
if __name__ == "__main__":
    print(get_current_weather("San Francisco"))
    print(get_current_weather("New York"))
    print(get_current_weather("London"))


