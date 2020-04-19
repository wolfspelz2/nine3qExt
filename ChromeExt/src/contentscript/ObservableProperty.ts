﻿export interface IObserver
{
    update(name: string, value: any): void;
}

export interface IObservable
{
    attach(observer: IObserver): void;
    detach(observer: IObserver): void;
}

export class ObservableProperty implements IObservable
{
    public value: any;
    protected observers: IObserver[] = [];

    constructor(private name: string) { }

    attach(observer: IObserver): void
    {
        const isExist = this.observers.includes(observer);
        if (!isExist) {
            // console.log('IObservable: Attached an observer.');
            this.observers.push(observer);
        }
    }

    detach(observer: IObserver): void
    {
        const observerIndex = this.observers.indexOf(observer);
        if (observerIndex === -1) {
            // console.log('IObservable: Nonexistent observer.');
        } else {
            this.observers.splice(observerIndex, 1);
            // console.log('IObservable: Detached an observer.');
        }
    }

    set(value: any): void
    {
        if (value === this.value) {
            // unchanged
        } else {
            this.value = value;
            this.notify();
        }
    }

    notifyOne(observer: IObserver): void
    {
        if (this.value != undefined) {
            observer.update(this.name, this.value);
        }
    }

    private notify(): void
    {
        // console.log('IObservable: Notifying observers', this.name);
        for (const observer of this.observers) {
            if (this.value != undefined) {
                observer.update(this.name, this.value);
            }
        }
    }
}
