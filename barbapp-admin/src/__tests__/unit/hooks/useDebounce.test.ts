import { renderHook, act } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { useDebounce } from '@/hooks/useDebounce';

describe('useDebounce', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('should return the initial value immediately', () => {
    const { result } = renderHook(() => useDebounce('initial', 500));

    expect(result.current).toBe('initial');
  });

  it('should debounce value changes', () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: 'initial', delay: 500 } }
    );

    // Change value
    rerender({ value: 'changed', delay: 500 });

    // Value should not change immediately
    expect(result.current).toBe('initial');

    // Fast-forward time
    act(() => {
      vi.advanceTimersByTime(500);
    });

    // Now value should be updated
    expect(result.current).toBe('changed');
  });

  it('should reset timer when value changes again', () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: 'initial', delay: 500 } }
    );

    // Change value
    rerender({ value: 'first', delay: 500 });

    // Advance time partially
    act(() => {
      vi.advanceTimersByTime(300);
    });

    // Change value again before debounce completes
    rerender({ value: 'second', delay: 500 });

    // Advance remaining time
    act(() => {
      vi.advanceTimersByTime(500);
    });

    // Should return the latest value
    expect(result.current).toBe('second');
  });

  it('should work with different delay values', () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: 'initial', delay: 1000 } }
    );

    rerender({ value: 'changed', delay: 1000 });

    // Advance time by 500ms - should not update yet
    act(() => {
      vi.advanceTimersByTime(500);
    });

    expect(result.current).toBe('initial');

    // Advance remaining time
    act(() => {
      vi.advanceTimersByTime(500);
    });

    expect(result.current).toBe('changed');
  });

  it('should handle different data types', () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: 42, delay: 300 } }
    );

    rerender({ value: 100, delay: 300 });

    act(() => {
      vi.advanceTimersByTime(300);
    });

    expect(result.current).toBe(100);
  });
});