﻿interface IObserver
{
    update(subject: IObservable): void;
}
interface IObservable
{
    attach(observer: IObserver): void;
    detach(observer: IObserver): void;
}

class Observable implements IObservable
{
    public value: any;
    private observers: IObserver[] = [];

    constructor(private name: string) { }

    public attach(observer: IObserver): void
    {
        const isExist = this.observers.includes(observer);
        if (!isExist) {
            // console.log('IObservable: Attached an observer.');
            this.observers.push(observer);
        }
    }

    public detach(observer: IObserver): void
    {
        const observerIndex = this.observers.indexOf(observer);
        if (observerIndex === -1) {
            // console.log('IObservable: Nonexistent observer.');
        } else {
            this.observers.splice(observerIndex, 1);
            console.log('IObservable: Detached an observer.');
        }
    }

    private notify(): void
    {
        // console.log('IObservable: Notifying observers', this.name);
        for (const observer of this.observers) {
            observer.update(this);
        }
    }

    public set(value: any): void
    {
        if (value === this.value) {
            // unchanged
        } else {
            this.value = value;
            this.notify();
        }
    }
}
