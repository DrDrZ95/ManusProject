import React, { Component, ErrorInfo, ReactNode } from 'react';
import { ErrorPage } from './ErrorPage';

interface Props {
  children?: ReactNode;
}

interface State {
  hasError: boolean;
}

/**
 * ErrorBoundary component to catch runtime errors in the component tree.
 */
export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = {
      hasError: false
    };
  }

  public static getDerivedStateFromError(_: Error): State {
    return { hasError: true };
  }

  public componentDidCatch(error: Error, errorInfo: ErrorInfo) {
    console.error("Uncaught error:", error, errorInfo);
  }

  public render() {
    // Accessing state via the class property
    if (this.state.hasError) {
      return <ErrorPage />;
    }

    // Accessing props via the base class property
    return this.props.children;
  }
}