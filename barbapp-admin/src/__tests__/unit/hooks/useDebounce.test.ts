import { renderHook, act } from '@testing-library/react';
import { vi, describe, it, expect, beforeEach, afterEach } from 'vitest';
import { useDebounce } from '@/hooks/useDebounce';

// Mock timers
beforeEach(() => {
  vi.useFakeTimers();
});

afterEach(() => {
  vi.useRealTimers();
});

describe('useDebounce', () => {
  it('should return the initial value immediately', () => {
    const { result } = renderHook(() => useDebounce('initial', 500));

    expect(result.current).toBe('initial');
  });

  it('should return the initial value when value changes immediately', () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: 'initial', delay: 500 } }
    );

    expect(result.current).toBe('initial');

    // Change value immediately
    rerender({ value: 'changed', delay: 500 });

    // Should still return old value immediately
    expect(result.current).toBe('initial');
  });

  it('should return the new value after delay', () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: 'initial', delay: 500 } }
    );

    // Change value
    rerender({ value: 'changed', delay: 500 });

    // Fast-forward time
    act(() => {
      vi.advanceTimersByTime(500);
    });

    expect(result.current).toBe('changed');
  });

  it('should reset timer when value changes again before delay', () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: 'initial', delay: 500 } }
    );

    // Change value
    rerender({ value: 'first change', delay: 500 });

    // Advance time partially
    act(() => {
      vi.advanceTimersByTime(300);
    });

    // Change value again before delay completes
    rerender({ value: 'second change', delay: 500 });

    // Advance time to complete the full delay from the second change
    act(() => {
      vi.advanceTimersByTime(500);
    });

    expect(result.current).toBe('second change');
  });

  it('should handle different delay values', () => {
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

  it('should handle zero delay', () => {
    const { result, rerender } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: 'initial', delay: 0 } }
    );

    rerender({ value: 'changed', delay: 0 });

    // With zero delay, should update immediately (next tick)
    act(() => {
      vi.advanceTimersByTime(0);
    });

    expect(result.current).toBe('changed');
  });

  it('should work with different data types', () => {
    // Test with number
    const { result: numberResult, rerender: rerenderNumber } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: 42, delay: 300 } }
    );

    rerenderNumber({ value: 100, delay: 300 });

    act(() => {
      vi.advanceTimersByTime(300);
    });

    expect(numberResult.current).toBe(100);

    // Test with boolean
    const { result: boolResult, rerender: rerenderBool } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: false, delay: 200 } }
    );

    rerenderBool({ value: true, delay: 200 });

    act(() => {
      vi.advanceTimersByTime(200);
    });

    expect(boolResult.current).toBe(true);

    // Test with object
    const obj1 = { name: 'John' };
    const obj2 = { name: 'Jane' };

    const { result: objResult, rerender: rerenderObj } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: obj1, delay: 100 } }
    );

    rerenderObj({ value: obj2, delay: 100 });

    act(() => {
      vi.advanceTimersByTime(100);
    });

    expect(objResult.current).toBe(obj2);
  });

  it('should clear timeout on unmount', () => {
    const clearTimeoutSpy = vi.spyOn(global, 'clearTimeout');

    const { unmount, rerender } = renderHook(
      ({ value, delay }) => useDebounce(value, delay),
      { initialProps: { value: 'initial', delay: 500 } }
    );

    rerender({ value: 'changed', delay: 500 });

    unmount();

    expect(clearTimeoutSpy).toHaveBeenCalled();
  });
});