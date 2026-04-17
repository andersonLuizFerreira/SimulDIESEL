#include "core/runtime/UceStatus.h"

void UceStatus::markBootCompleted() {
  _bootCompleted = true;
}

void UceStatus::markTransportReady(bool ready) {
  _transportReady = ready;
}

void UceStatus::markServiceReady(bool ready) {
  _serviceReady = ready;
}

void UceStatus::markLinkReady(bool ready) {
  _linkReady = ready;
}

void UceStatus::markCanReady(bool ready) {
  _canReady = ready;
}

void UceStatus::bumpLoopCounter() {
  ++_loopCounter;
}

bool UceStatus::bootCompleted() const {
  return _bootCompleted;
}

bool UceStatus::transportReady() const {
  return _transportReady;
}

bool UceStatus::serviceReady() const {
  return _serviceReady;
}

bool UceStatus::linkReady() const {
  return _linkReady;
}

bool UceStatus::canReady() const {
  return _canReady;
}

uint32_t UceStatus::loopCounter() const {
  return _loopCounter;
}
