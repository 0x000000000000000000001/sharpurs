// Node filesystem errors carry a stable code; do not classify message text.
export const hasErrorCode = code => error => error.code === code;

// Some Node failures (e.g. reading a directory) omit the path entirely.
export const pathError = path => cause => Object.assign(
  new Error(`Sharpurs project '${path}': ${cause.message}`, { cause }),
  { code: cause.code, syscall: cause.syscall, path }
);
