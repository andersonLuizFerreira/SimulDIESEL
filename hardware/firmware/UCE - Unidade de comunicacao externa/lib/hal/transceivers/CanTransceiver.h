#pragma once

class CanTransceiver {
public:
  virtual ~CanTransceiver();

  virtual bool enable() = 0;
  virtual bool disable() = 0;
  virtual bool standby() = 0;
  virtual bool wake() = 0;
  virtual bool hasFault() const = 0;
  virtual bool isPresent() const = 0;
};
