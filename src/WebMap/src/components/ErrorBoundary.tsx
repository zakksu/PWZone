import { Component, type ErrorInfo, type ReactNode } from 'react';

interface Props {
  children: ReactNode;
  label?: string;
}

interface State {
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error(`[PW Companion] ${this.props.label ?? 'UI'} crashed:`, error, info);
  }

  render() {
    if (this.state.error) {
      return (
        <div style={{
          padding: 24,
          color: '#eaeaea',
          background: '#1a1a2e',
          height: '100%',
          overflow: 'auto',
        }}>
          <h2 style={{ color: '#e94560', marginTop: 0 }}>
            {this.props.label ?? 'Something'} broke
          </h2>
          <p>The butler tripped over a bug. Details below — screenshot this if you need help.</p>
          <pre style={{
            background: '#0f0f1a',
            padding: 16,
            borderRadius: 8,
            fontSize: 12,
            whiteSpace: 'pre-wrap',
          }}>
            {this.state.error.message}
          </pre>
        </div>
      );
    }

    return this.props.children;
  }
}
