#pragma once

#include <stdint.h>

class UceStatus {
public:
  void markBootCompleted();
  void markTransportReady(bool ready);
  void markServiceReady(bool ready);
  void markLinkReady(bool ready);
  void markCanReady(bool ready);
  void bumpLoopCounter();

  bool bootCompleted() const;
  bool transportReady() const;
  bool serviceReady() const;
  bool linkReady() const;
  bool canReady() const;
  uint32_t loopCounter() const;

private:
  bool _bootCompleted = false;
  bool _transportReady = false;
  bool _serviceReady = false;
  bool _linkReady = false;
  bool _canReady = false;
  uint32_t _loopCounter = 0;
};
