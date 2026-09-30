export const track = label => value => {
  track.events.push(label);
  return value;
};

// The harness reads the log without adding a foreign function to the source API.
track.events = [];
