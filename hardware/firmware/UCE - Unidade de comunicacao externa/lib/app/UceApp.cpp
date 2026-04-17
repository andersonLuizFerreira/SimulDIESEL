#include "app/UceApp.h"

#include "diag/trace/DiagTrace.h"
#include "services/can/CanConfig.h"

UceApp::UceApp()
  : _transport(),
    _led(),
    _canTransceiver(),
    _canDriver(),
    _canService(_canDriver, _canTransceiver),
    _service(_led, _canService),
    _link(_transport, _service),
    _context()
{
}

void UceApp::begin() {
  DiagTrace::begin();

  _led.begin();
  _service.begin();
  _transport.begin();
  _link.begin();

  _canService.begin();
  _canService.configure(CanConfig{});

  UceStatus& status = _context.status();
  status.markBootCompleted();
  status.markTransportReady(true);
  status.markServiceReady(true);
  status.markLinkReady(true);
  status.markCanReady(true);
}

void UceApp::tick() {
  _link.tick();
  _context.status().bumpLoopCounter();
}

void UceApp::poll() {
  _link.poll();
}

UceContext& UceApp::context() {
  return _context;
}

const UceContext& UceApp::context() const {
  return _context;
}
