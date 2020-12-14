import { useEffect, useState, useCallback, useRef } from "react";
import { useDispatch } from "react-redux";
import { setLoading } from "../store/slices/app-slice";

const useAsync = <T extends {}>(asyncFunction: () => Promise<T>, initValue: T, immediate = true) => {
  const [pending, setPending] = useState(false);
  const [value, setValue] = useState(initValue);
  const [error, setError] = useState<Error | null>(null);
  const dispatch = useDispatch();

  const execute = useCallback(async () => {
    dispatch(setLoading(true));
    setPending(true);
    setError(null);
    try {
      var response = await asyncFunction();
      setValue(response);
    } catch (error) {
      setError(error);
    } finally {
      setPending(false);
      dispatch(setLoading(false));
    }
  }, [asyncFunction]);

  useEffect(() => {
    if (immediate) {
      execute();
    }
  }, [execute, immediate]);

  return { execute, pending, value, error };
};

const usePrevious = <T extends {}>(value: T) => {
  // The ref object is a generic container whose current property is mutable ...
  // ... and can hold any value, similar to an instance property on a class
  const ref = useRef<T>();

  // Store current value in ref
  useEffect(() => {
    ref.current = value;
  }, [value]); // Only re-run if value changes

  // Return previous value (happens before update in useEffect above)
  return ref.current;
};

export { useAsync, usePrevious };
